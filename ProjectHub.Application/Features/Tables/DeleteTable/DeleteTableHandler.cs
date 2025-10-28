using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Tables.DeleteTable;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    public class DeleteTableHandler : IRequestHandler<DeleteTableCommand, Unit>
    {
        private readonly ITableRepository _tableRepository;

        public DeleteTableHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<Unit> Handle(
            DeleteTableCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ดึงข้อมูล Project ที่ต้องการลบ
            var tableToDelete = await _tableRepository.GetTableByIdAsync(request.TableId);

            // 2. ตรวจสอบว่าเจอหรือไม่
            if (tableToDelete == null)
            {
                throw new ArgumentException($"Project with ID {request.TableId} not found.");
                // หรือใช้ NotFoundException
            }

            // (Optional) ตรวจสอบ Business Rule อื่นๆ ก่อนลบได้
            // เช่น เช็คว่า User ที่ขอลบ เป็นเจ้าของ Project หรือไม่ (ถ้ามีระบบ Auth)

            // 3. เรียก Repository เพื่อทำการลบ
            await _tableRepository.DeleteTableAsync(tableToDelete);

            // 4. คืนค่า Unit.Value เพื่อบอกว่าสำเร็จ (ไม่มีข้อมูลส่งกลับ)
            return Unit.Value;
        }
    }
}
