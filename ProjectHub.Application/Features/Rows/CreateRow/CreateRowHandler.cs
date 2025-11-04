using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // 1. [ADD] เพิ่ม Using นี้
using System.Threading;
using System.Threading.Tasks;
using ProjectHub.Application.Validation;

// (Alias ที่คุณมีอยู่แล้ว)
using ColumnEntity = ProjectHub.Domain.Entities.Columns;
using RowEntity = ProjectHub.Domain.Entities.Rows;
using TableEntity = ProjectHub.Domain.Entities.Tables;


namespace ProjectHub.Application.Features.Rows.CreateRow
{
    public class CreateRowHandler : IRequestHandler<CreateRowCommand, RowResponseDto>
    {
        private readonly IRowRepository _rowRepository;
        private readonly ITableRepository _tableRepository; 
        private readonly IColumnRepository _columnRepository; 
        private readonly IMapper _mapper;
        private readonly IProjectSecurityService _securityService;

        public CreateRowHandler(
            IRowRepository rowRepository,
            ITableRepository tableRepository,
            IColumnRepository columnRepository,
            IMapper mapper,
            IProjectSecurityService securityService
            )

        {
            _rowRepository = rowRepository;
            _tableRepository = tableRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<RowResponseDto> Handle(CreateRowCommand request, CancellationToken cancellationToken)
        {
            // --- 1. ตรวจสอบสิทธิ์ ---
            await _securityService.ValidateTableAccessAsync(request.TableId);
            
            // --- 2. ตรวจสอบ Table และดึง Schema ---
            var tableExists = await _tableRepository.GetTableByIdAsync(request.TableId);
            if (tableExists == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
            }

            var columnsSchema = (await _columnRepository.GetColumnsByTableIdAsync(request.TableId)).ToList();
            if (!columnsSchema.Any())
            {
                 // (อนุญาตให้สร้าง Row ในตารางที่ไม่มี Column)
            }

            // --- 3. [LOGIC ใหม่] จัดการ Primary Key ---
            var pkColumn = columnsSchema.FirstOrDefault(c => c.Is_primary);
            string originalRequestData = request.Data; // เก็บ JSON ดั้งเดิมไว้เผื่อ
            
            if (pkColumn != null)
            {
                // 3.1. กรณี "AUTO_INCREMENT"
                if (pkColumn.PrimaryKeyType == "AUTO_INCREMENT")
                {
                    // (คุณต้องไปสร้าง Method นี้ใน IRowRepository)
                    var maxId = await _rowRepository.GetMaxPkValueAsync(request.TableId, pkColumn.Name);
                    var newId = maxId + 1;

                    // "ยัด" ID ใหม่นี้เข้าไปใน JSON Data
                    request.Data = InjectValueIntoJson(request.Data, pkColumn.Name, newId);
                }
                // 3.2. กรณี "MANUAL"
                else if (pkColumn.PrimaryKeyType == "MANUAL")
                {
                    // (คุณต้องไปสร้าง Method นี้ใน IRowRepository)
                    var pkValue = GetValueFromJson(request.Data, pkColumn.Name);
                    var isDuplicate = await _rowRepository.IsPkValueDuplicateAsync(
                        request.TableId, 
                        pkColumn.Name, 
                        pkValue 
                    );

                    if (isDuplicate)
                    {
                        throw new ArgumentException($"ข้อมูลซ้ำ! ค่า Primary Key '{pkValue}' ถูกใช้ไปแล้ว");
                    }
                }
            }
            
            // --- 4. Validate JSON Data (เวอร์ชันสุดท้าย) ---
            try
            {
                // (เรา Validate "หลัง" จากยัด ID เข้าไปแล้ว)
                JsonDataValidator.Validate(request.Data, columnsSchema.ToList());
            }
            catch (Exception)
            {
                request.Data = originalRequestData; // กู้คืน JSON เดิมถ้าพัง
                throw; // โยน Error เดิมกลับไป
            }

            // --- 5. Mapping & Persistence ---
            var rowEntity = _mapper.Map<RowEntity>(request);
            // (ถ้า Mapper ไม่ได้ Map 'Data' ให้... คุณต้อง Map เอง)
            // rowEntity.Data = request.Data; 
            
            await _rowRepository.AddRowAsync(rowEntity);

            // --- 6. Mapping & Response ---
            var responseDto = _mapper.Map<RowResponseDto>(rowEntity);
            responseDto.CreatedAt = rowEntity.Created_at;

            return responseDto;
        }

        // --- Helper Methods (วางไว้ท้าย Class) ---

        private string InjectValueIntoJson(string jsonString, string key, object value)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement.EnumerateObject();
                var dict = root.ToDictionary(prop => prop.Name, prop => prop.Value.Clone());
                dict[key] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
                return JsonSerializer.Serialize(dict);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}");
            }
        }

        private string GetValueFromJson(string jsonString, string key)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonString);
                if (jsonDoc.RootElement.TryGetProperty(key, out var valueElement))
                {
                    if (valueElement.ValueKind == JsonValueKind.Null)
                        throw new ArgumentException($"Primary Key '{key}' cannot be null.");
                    
                    return valueElement.ToString();
                }
                throw new ArgumentException($"ไม่พบ Primary Key '{key}' ในข้อมูลที่ส่งมา");
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}");
            }
        }
    }
}