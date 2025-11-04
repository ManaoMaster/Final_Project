using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace TableHub.Application.Features.Tables.DeleteTable
{
    // Handler for DeleteTableCommand, returns Unit (nothing)
    public class DeleteTableHandler : IRequestHandler<DeleteTableCommand, Unit>
    {
        private readonly ITableRepository _tableRepository;


        private readonly IProjectSecurityService _securityService;

        public DeleteTableHandler(ITableRepository tableRepository, IProjectSecurityService securityService)
        {
            _tableRepository = tableRepository;
            _securityService = securityService;
        }

        public async Task<Unit> Handle(
            DeleteTableCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ดึงตาราง (ครั้งที่ 1 และครั้งเดียว)
            var tableToDelete = await _tableRepository.GetTableByIdAsync(request.TableId);

            if (tableToDelete == null)
            {
                throw new ArgumentException($"Table with ID {request.TableId} not found.");
            }

            // 2. [FIX] เรียก "ยาม" โดยใช้ ProjectId ที่เราเพิ่งหาเจอ
            // (เราข้าม 'ValidateTableAccessAsync' ไปเลย เพราะเรามีข้อมูลแล้ว)
            await _securityService.ValidateProjectAccessAsync(tableToDelete.Project_id); // (หรือ .ProjectId)

            // 3. (ถ้าผ่าน) ลบตาราง
            await _tableRepository.DeleteTableAsync(request.TableId);

            return Unit.Value;
        }
    }
}
