using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities; // ใช้ Entity Rows และ Columns
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // สำหรับ JSON Validation
using System.Threading;
using System.Threading.Tasks;

// ใช้ Alias ป้องกัน Namespace Collision กับ Features.Columns
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using RowEntity = ProjectHub.Domain.Entities.Rows;
using TableEntity = ProjectHub.Domain.Entities.Tables;


namespace ProjectHub.Application.Features.Rows.CreateRow
{
    public class CreateRowHandler : IRequestHandler<CreateRowCommand, RowResponseDto>
    {
        private readonly IRowRepository _rowRepository;
        private readonly ITableRepository _tableRepository; // Inject เพื่อเช็ค Table
        private readonly IColumnRepository _columnRepository; // Inject เพื่อดึง Schema
        private readonly IMapper _mapper;

        public CreateRowHandler(
            IRowRepository rowRepository,
            ITableRepository tableRepository,
            IColumnRepository columnRepository,
            IMapper mapper)
        {
            _rowRepository = rowRepository;
            _tableRepository = tableRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
        }

        public async Task<RowResponseDto> Handle(CreateRowCommand request, CancellationToken cancellationToken)
        {
            // --- 1. ตรวจสอบว่า TableId มีอยู่จริง ---
            var tableExists = await _tableRepository.GetTableByIdAsync(request.TableId);
            if (tableExists == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
            }

            // --- 2. ดึง Schema ของ Columns สำหรับ Table นี้ ---
            var columnsSchema = await _columnRepository.GetColumnsByTableIdAsync(request.TableId);
            if (!columnsSchema.Any())
            {
                // อาจจะอนุญาตให้สร้าง Row ในตารางที่ไม่มี Column ก็ได้ แล้วแต่ Requirement
                // throw new ArgumentException($"Table with ID {request.TableId} has no columns defined.");
            }

            // --- 3. Validate JSON Data ---
            try
            {
                ValidateJsonDataAgainstSchema(request.Data, columnsSchema);
            }
            catch (JsonException jsonEx)
            {
                throw new ArgumentException($"Invalid JSON format: {jsonEx.Message}");
            }
            catch (ArgumentException argEx) // Catch Validation Errors from our method
            {
                throw argEx; // Re-throw to return 400 Bad Request
            }

            // --- 4. Mapping & Persistence ---
            // Map Command -> Entity Rows
            var rowEntity = _mapper.Map<RowEntity>(request);
            // Created_at ถูกกำหนด Default ใน Entity แล้ว

            // เรียก Repository เพื่อบันทึก
            await _rowRepository.AddRowAsync(rowEntity);

            // --- 5. Mapping & Response ---
            // Map Entity -> Response DTO
            var responseDto = _mapper.Map<RowResponseDto>(rowEntity);
            // ดึงค่า Created_at ที่ Database อาจจะ Generate ให้ (ถ้ามีการเปลี่ยนแปลง)
            responseDto.CreatedAt = rowEntity.Created_at; 

            return responseDto;
        }

        // --- Helper Method for JSON Validation ---
        private void ValidateJsonDataAgainstSchema(string jsonDataString, IEnumerable<ColumnEntity> schema)
        {
            if (string.IsNullOrWhiteSpace(jsonDataString))
            {
                throw new ArgumentException("Row data (JSON) cannot be empty.");
            }

            using JsonDocument jsonData = JsonDocument.Parse(jsonDataString);
            var root = jsonData.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Row data must be a JSON object.");
            }

            // เก็บชื่อ Property ที่เจอใน JSON เพื่อเช็ค Key เกิน
            var jsonPropertiesPresent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // วน Loop ผ่าน Properties ใน JSON ที่ Client ส่งมา
            foreach (var jsonProperty in root.EnumerateObject())
            {
                jsonPropertiesPresent.Add(jsonProperty.Name);

                // หา Column ที่ตรงกับชื่อ Property ใน Schema (ไม่สนตัวพิมพ์เล็ก/ใหญ่)
                var columnDefinition = schema.FirstOrDefault(c => c.Name.Equals(jsonProperty.Name, StringComparison.OrdinalIgnoreCase));

                // 1. เช็ค Key เกิน: ถ้า Property ใน JSON ไม่มีใน Schema
                if (columnDefinition == null)
                {
                    throw new ArgumentException($"Invalid column name '{jsonProperty.Name}' found in JSON data. It does not exist in the table schema.");
                }

                var valueElement = jsonProperty.Value;

                // 2. เช็ค Null: ถ้า Value เป็น null แต่ Schema ไม่อนุญาต
                if (valueElement.ValueKind == JsonValueKind.Null && !columnDefinition.Is_nullable)
                {
                    throw new ArgumentException($"Column '{columnDefinition.Name}' cannot be null.");
                }

                // 3. เช็ค Data Type (ถ้าไม่ Null)
                if (valueElement.ValueKind != JsonValueKind.Null)
                {
                    bool typeMatch = columnDefinition.Data_type.ToUpperInvariant() switch
                    {
                        // คุณอาจต้องปรับแก้ Type String เหล่านี้ให้ตรงกับที่คุณใช้
                        "TEXT" => valueElement.ValueKind == JsonValueKind.String,
                        "INTEGER" => valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt64(out _),
                        "REAL" => valueElement.ValueKind == JsonValueKind.Number, // Number covers Int & Float/Double
                        "BOOLEAN" => valueElement.ValueKind == JsonValueKind.True || valueElement.ValueKind == JsonValueKind.False,
                        // เพิ่ม Type อื่นๆ เช่น "DATE", "DATETIME" และ Logic การ Parse/Validate
                        _ => throw new ArgumentException($"Unsupported data type '{columnDefinition.Data_type}' defined in schema for column '{columnDefinition.Name}'.") // หรือจะ Skip การเช็ค Type ที่ไม่รู้จัก
                    };

                    if (!typeMatch)
                    {
                        throw new ArgumentException($"Invalid data type for column '{columnDefinition.Name}'. Expected '{columnDefinition.Data_type}' but received '{valueElement.ValueKind}'.");
                    }
                }
            }

            // 4. เช็ค Key ขาด: วน Loop ผ่าน Schema เพื่อดูว่ามี Column ไหนที่จำเป็น (Not Nullable) แต่ไม่มีใน JSON หรือไม่
            foreach (var column in schema)
            {
                if (!column.Is_nullable && !jsonPropertiesPresent.Contains(column.Name))
                {
                    throw new ArgumentException($"Required column '{column.Name}' is missing from JSON data.");
                }
            }
        }
    }
}

