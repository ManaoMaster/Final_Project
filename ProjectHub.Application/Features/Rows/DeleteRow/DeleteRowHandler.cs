using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Features.Rows.DeleteRow; // 1. [FIX] แก้ Namespace ให้ถูกต้อง
using ProjectHub.Application.Interfaces; // 2. [ADD] Import "ยาม"
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Rows.DeleteRow // 1. [FIX] แก้ Namespace ให้ถูกต้อง
{
    public class DeleteRowHandler : IRequestHandler<DeleteRowCommand, Unit>
    {
        private readonly IRowRepository _rowRepository;
        private readonly IProjectSecurityService _securityService; // 3. [ADD] Inject "ยาม"

        public DeleteRowHandler(
            IRowRepository rowRepository, 
            IProjectSecurityService securityService) // 4. [ADD] รับ "ยาม" ใน Constructor
        {
            _rowRepository = rowRepository;
            _securityService = securityService; // 5. [ADD] กำหนดค่า
        }

        public async Task<Unit> Handle(
            DeleteRowCommand request,
            CancellationToken cancellationToken
        )
        {
            // 6. [OPTIMIZE] ดึงแถวออกมาก่อน (1 DB Call)
            var rowToDelete = await _rowRepository.GetRowByIdAsync(request.RowId); // (สมมติว่าชื่อ Method นี้)
            
            if (rowToDelete == null)
            {
                throw new ArgumentException($"Row with ID {request.RowId} not found.");
            }

            // 7. [ADD] เรียก "ยาม" โดยใช้ข้อมูลที่เราเพิ่งดึงมา
            // (เราข้าม 'ValidateTableAccessAsync' ไป... 
            //  เพราะมันจะไปดึง Table ซึ่งเราไม่จำเป็นต้องใช้ ProjectId ก็ได้)
            //  เราแค่เช็คว่า User มีสิทธิ์ใน Table ที่ Row นี้อยู่หรือไม่
            await _securityService.ValidateTableAccessAsync(rowToDelete.Table_id); // (หรือ .TableId)

            // 8. (ถ้าผ่าน) สั่งลบ
            await _rowRepository.DeleteRowAsync(request.RowId);

            return Unit.Value;
        }
    }
}