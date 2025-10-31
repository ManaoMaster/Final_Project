using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;


// (อาจจะต้องเพิ่ม Exception ที่คุณสร้างเอง เช่น NotFoundException)

namespace ProjectHub.Application.Features.Relationships.UpdateRelationship
{
    public class UpdateRelationshipHandler : IRequestHandler<UpdateRelationshipCommand>
    {
        private readonly IRelationshipRepository _relationshipRepository;

        public UpdateRelationshipHandler(IRelationshipRepository relationshipRepository)
        {
            _relationshipRepository = relationshipRepository;
        }

        public async Task Handle(
            UpdateRelationshipCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ค้นหา Relationship ที่มีอยู่
            var relationshipToUpdate = await _relationshipRepository.GetByIdAsync(request.Id);

            // 2. ตรวจสอบว่าเจหรือไม่
            if (relationshipToUpdate == null)
            {
                // โยน Exception ที่นี่ (เช่น throw new NotFoundException(...))
                // หรือ return ผลลัพธ์ที่เป็น Error ตามที่คุณออกแบบไว้
                throw new Exception($"Relationship with ID {request.Id} not found.");
            }

            // 3. อัปเดตเฉพาะ Metadata (เราจะไม่แตะต้อง Key ใดๆ ทั้งสิ้น)
            relationshipToUpdate.DisplayName = request.DisplayName;
            relationshipToUpdate.Notes = request.Notes;

            // 4. บันทึกการเปลี่ยนแปลง
            await _relationshipRepository.UpdateAsync(relationshipToUpdate);
        }
    }
}
