using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities; // <-- Entity หลัก
// *** เพิ่ม: สร้าง "ชื่อเล่น" (Alias) ให้กับ Entity 'Relationships' ***
// *** เพื่อป้องกันชื่อชนกับ Namespace 'Features.Relationships' ***
using RelationshipEntity = ProjectHub.Domain.Entities.Relationships;

namespace ProjectHub.Application.Features.Relationships.CreateRelationship
{
    public class CreateRelationshipHandler
        : IRequestHandler<CreateRelationshipCommand, RelationshipResponseDto>
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IColumnRepository _columnRepository; // Inject เพื่อ Validate Columns
        private readonly IMapper _mapper;

        public CreateRelationshipHandler(
            IRelationshipRepository relationshipRepository,
            IColumnRepository columnRepository, // ต้องใช้ IColumnRepository
            IMapper mapper
        )
        {
            _relationshipRepository = relationshipRepository;
            _columnRepository = columnRepository;
            _mapper = mapper;
        }

        public async Task<RelationshipResponseDto> Handle(
            CreateRelationshipCommand request,
            CancellationToken cancellationToken
        )
        {
            // --- 1. Validation (ตรวจสอบความถูกต้องของ "กฎ") ---

            if (request.PrimaryColumnId == request.ForeignColumnId)
            {
                throw new ArgumentException(
                    "Cannot create a relationship from a column to itself."
                );
            }

            // ดึงข้อมูล Column ต้นทาง (PK) และปลายทาง (FK)
            var primaryColumn = await _columnRepository.GetColumnByIdAsync(request.PrimaryColumnId);
            var foreignColumn = await _columnRepository.GetColumnByIdAsync(request.ForeignColumnId);

            // เช็คว่า Column มีอยู่จริง
            if (primaryColumn == null)
                throw new ArgumentException(
                    $"Primary Column with ID {request.PrimaryColumnId} not found."
                );
            if (foreignColumn == null)
                throw new ArgumentException(
                    $"Foreign Column with ID {request.ForeignColumnId} not found."
                );

            // เช็คว่า Table ID ใน Request ตรงกับ Table ID ของ Column จริงๆ
            if (primaryColumn.Table_id != request.PrimaryTableId)
                throw new ArgumentException(
                    "Primary Column does not belong to the specified Primary Table."
                );
            if (foreignColumn.Table_id != request.ForeignTableId)
                throw new ArgumentException(
                    "Foreign Column does not belong to the specified Foreign Table."
                );

            // เช็คว่า Table ต้นทางกับปลายทาง ห้ามเป็นตารางเดียวกัน (ป้องกัน Loop)
            if (request.PrimaryTableId == request.ForeignTableId)
            {
                throw new ArgumentException("Cannot create a relationship within the same table.");
            }

            // --- 2. ตรวจสอบ Business Logic (ตามที่คุณต้องการ) ---

            // เช็คว่า Column ต้นทาง (PrimaryColumn) ถูกตั้งเป็น Primary Key หรือไม่
            if (primaryColumn.Is_primary == false)
            {
                // เราอนุญาตให้เชื่อมจาก Column ที่ไม่ใช่ PK ก็ได้ (แล้วแต่ Requirement)
                // แต่ถ้าบังคับว่าต้องเป็น PK เท่านั้น ให้ uncomment บรรทัดนี้:
                // throw new ArgumentException($"Primary Column '{primaryColumn.Name}' must be a Primary Key to create a relationship.");
            }

            // เช็คว่า Data Type ตรงกันหรือไม่ (เช่น INTEGER ต้องเชื่อมกับ INTEGER)
            if (
                primaryColumn.Data_type?.ToUpperInvariant()
                != foreignColumn.Data_type?.ToUpperInvariant()
            )
            {
                throw new ArgumentException(
                    $"Data type mismatch. Cannot link {primaryColumn.Data_type} (Primary) to {foreignColumn.Data_type} (Foreign)."
                );
            }

            // (Optional: เช็คว่า "กฎ" นี้ซ้ำซ้อนกับที่มีอยู่หรือไม่)

            // --- 3. Mapping & Persistence ---
            // *** แก้ไข: ใช้ "ชื่อเล่น" (Alias) ที่เราตั้ง ***
            var relationshipEntity = _mapper.Map<RelationshipEntity>(request);

            await _relationshipRepository.AddRelationshipAsync(relationshipEntity);

            // --- 4. Response ---
            // *** แก้ไข: ใช้ "ชื่อเล่น" (Alias) ที่เราตั้ง ***
            var responseDto = _mapper.Map<RelationshipResponseDto>(relationshipEntity);

            return responseDto;
        }
    }
}
