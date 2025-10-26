using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper; // เพิ่ม Using นี้
using ProjectHub.Domain.Entities;
using ProjectHub.Application.Features.Columns.CreateColumn;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Dtos; // ใช้ Entity Columns

namespace ProjectHub.Application.Features.Columns.CreateColumn
{
    public class CreateColumnHandler : IRequestHandler<CreateColumnCommand, ColumnResponseDto>
    {
        private readonly IColumnRepository _columnRepository;
        private readonly ITableRepository _tableRepository; // เพิ่ม Repository นี้เข้ามา
        private readonly IMapper _mapper;

        public CreateColumnHandler(
            IColumnRepository columnRepository,
            ITableRepository tableRepository, // เพิ่ม Parameter นี้
            IMapper mapper)
        {
            _columnRepository = columnRepository;
            _tableRepository = tableRepository; // เพิ่มการ Assign ค่า
            _mapper = mapper;
        }

        public async Task<ColumnResponseDto> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
        {
            // --- Business Logic ---

            // 1. ตรวจสอบว่า TableId มีอยู่จริงหรือไม่
            // (ต้องไปเพิ่ม GetTableByIdAsync ใน ITableRepository และ TableRepository ด้วย)
            // var tableExists = await _tableRepository.GetTableByIdAsync(request.TableId);
            // if (tableExists == null)
            // {
            //     throw new ArgumentException($"Table with ID {request.TableId} not found.");
            // }
            // **หมายเหตุ:** การเช็ค Table อาจไม่จำเป็น ถ้า Flow UI บังคับว่าต้องอยู่ใน Table ก่อน

            // 2. ตรวจสอบ Input พื้นฐาน
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Column name is required.");
            if (string.IsNullOrWhiteSpace(request.DataType))
                throw new ArgumentException("Data type is required.");

            // 3. ตรวจสอบชื่อ Column ซ้ำใน Table นี้
            var isDuplicateName = await _columnRepository.IsColumnNameUniqueForTableAsync(request.TableId, request.Name);
            if (isDuplicateName)
                throw new ArgumentException($"Column name '{request.Name}' already exists in this table.");

            // 4. ตรวจสอบ Primary Key ซ้ำ (ถ้า User เลือก IsPrimary)
            if (request.IsPrimary)
            {
                var hasPrimaryKey = await _columnRepository.HasPrimaryKeyAsync(request.TableId);
                if (hasPrimaryKey)
                    throw new ArgumentException("This table already has a primary key defined.");
            }

            // --- Mapping & Persistence ---

            // 5. ใช้ AutoMapper แปลง Command -> Entity Columns
            // ใช้ชื่อเต็มเพื่อป้องกัน Namespace Collision
            var columnEntity = _mapper.Map<ProjectHub.Domain.Entities.Columns>(request);

            // 6. เรียก Repository เพื่อบันทึก Entity ลง Database
            await _columnRepository.AddColumnAsync(columnEntity);

            // 7. ใช้ AutoMapper แปลง Entity -> DTO เพื่อส่งกลับ
            var responseDto = _mapper.Map<ColumnResponseDto>(columnEntity);

            return responseDto;
        }
    }
}
