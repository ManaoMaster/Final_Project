using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using ProjectHub.Domain.Entities;
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Dtos; 
using System.Linq; // <-- ตรวจสอบว่ามี Using นี้สำหรับ .Any()

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, ColumnResponseDto>
    {
        // *** 1. แก้ไข: เพิ่มชื่อตัวแปร _formulaTranslator ***
        private readonly IFormulaTranslator _formulaTranslator; 
        private readonly IColumnRepository _columnRepository;
        private readonly ITableRepository _tableRepository; 
        private readonly IMapper _mapper;

        public CreateColumnHandler(
            IColumnRepository columnRepository,
            ITableRepository tableRepository, 
            IMapper mapper,
            IFormulaTranslator formulaTranslator // <-- *** 2. เพิ่ม Parameter นี้เข้ามา ***
        )
        {
            // *** 3. กำหนดค่าให้ Field _formulaTranslator (ตัวที่มี _) ***
            _formulaTranslator = formulaTranslator;
            _columnRepository = columnRepository;
            _tableRepository = tableRepository; 
            _mapper = mapper;
        }

        public async Task<ColumnResponseDto> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
        {
            // --- Business Logic (Logic ของคุณดีอยู่แล้ว) ---

            // 1. ตรวจสอบ Input พื้นฐาน
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required.");
            if (string.IsNullOrWhiteSpace(request.DataType))
                throw new ArgumentException("Data type is required.");

            // 2. ตรวจสอบชื่อ Column ซ้ำใน Table นี้
            // (คุณต้องสร้าง IsColumnNameUniqueForTableAsync ใน Repository)
            // var isDuplicateName = await _columnRepository.IsColumnNameUniqueForTableAsync(request.TableId, request.Name);
            // if (isDuplicateName)
            //     throw new ArgumentException($"Column name '{request.Name}' already exists in this table.");

            // 3. ตรวจสอบ Primary Key ซ้ำ (ถ้า User เลือก IsPrimary)
            if (request.IsPrimary)
            {
                // (คุณต้องสร้าง HasPrimaryKeyAsync ใน Repository)
                // var hasPrimaryKey = await _columnRepository.HasPrimaryKeyAsync(request.TableId);
                // if (hasPrimaryKey)
                //     throw new ArgumentException("This table already has a primary key defined.");
            }

            // --- Formula Validation (ถ้าจำเป็น) ---
            if (request.DataType == "Formula")
            {
                // (ตอนนี้ request.FormulaDefinition จะไม่แดงแล้ว ถ้าคุณแก้ไฟล์ Command)
                if (string.IsNullOrWhiteSpace(request.FormulaDefinition))
                {
                    throw new Exception("FormulaDefinition is required for Formula columns.");
                }

                // 3.1 ดึงชื่อคอลัมน์ทั้งหมดที่สูตรนี้อ้างอิง
                // *** ตอนนี้ _formulaTranslator จะไม่เป็น null แล้ว ***
                var columnNames = _formulaTranslator.GetReferencedColumnNames(request.FormulaDefinition);

                if (!columnNames.Any())
                {
                    throw new Exception("Formula does not reference any columns.");
                }

                // 3.2 ตรวจสอบทีละคอลัมน์
                foreach (var name in columnNames)
                {
                    // *** 4. แก้ไข: ใช้ request.TableId (T ใหญ่) ให้ตรงกัน ***
                    var colInfo = await _columnRepository.GetColumnByNameAsync(request.TableId, name);

                    if (colInfo == null)
                    {
                        throw new Exception($"Formula Error: Column '{name}' not found in this table.");
                    }

                    // (เช็ค Data_type ตามที่คุณบอกว่ามี 'integer' 'long')
                    if (colInfo.Data_type != "integer" && colInfo.Data_type != "long")
                    {
                        throw new Exception($"Formula Error: Column '{name}' is not a numeric type ('integer' or 'long').");
                    }
                }
            }
            
            // --- Mapping & Persistence ---

            // 5. ใช้ AutoMapper แปลง Command -> Entity Columns
            var columnEntity = _mapper.Map<ProjectHub.Domain.Entities.Columns>(request);

            // 6. เรียก Repository เพื่อบันทึก Entity ลง Database
            // *** 5. ลบบรรทัดที่ซ้ำซ้อนออก ***
            await _columnRepository.AddColumnAsync(columnEntity); // (สมมติว่าใช้เมธอดนี้)

            // 7. ใช้ AutoMapper แปลง Entity -> DTO เพื่อส่งกลับ
            var responseDto = _mapper.Map<ColumnResponseDto>(columnEntity);

            return responseDto;
        }
    }
}