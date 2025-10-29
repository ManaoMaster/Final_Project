using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Features.Columns.DeleteColumn;

namespace ProjectHub.Application.Features.Columns.DeleteColumns
{
    public class DeleteColumnHandler : IRequestHandler<DeleteColumnCommand, Unit>
    {
        private readonly IColumnRepository _ColumnRepository;

        public DeleteColumnHandler(IColumnRepository ColumnRepository)
        {
            _ColumnRepository = ColumnRepository;
        }

        public async Task<Unit> Handle(
            DeleteColumnCommand request,
            CancellationToken cancellationToken
        )
        {
            // 1. ดึงข้อมูล Column ที่ต้องการลบ
            var ColumnToDelete = await _ColumnRepository.GetColumnByIdAsync(request.ColumnId);

            // 2. ตรวจสอบว่าเจอหรือไม่
            if (ColumnToDelete == null)
            {
                throw new ArgumentException($"Column with ID {request.ColumnId} not found.");
                // หรือใช้ NotFoundException
            }

            // (Optional) ตรวจสอบ Business Rule อื่นๆ ก่อนลบได้
            // เช่น เช็คว่า User ที่ขอลบ เป็นเจ้าของ Column หรือไม่ (ถ้ามีระบบ Auth)

            // 3. เรียก Repository เพื่อทำการลบ
            await _ColumnRepository.DeleteColumnAsync(ColumnToDelete);

            // 4. คืนค่า Unit.Value เพื่อบอกว่าสำเร็จ (ไม่มีข้อมูลส่งกลับ)
            return Unit.Value;
        }
    }
}
