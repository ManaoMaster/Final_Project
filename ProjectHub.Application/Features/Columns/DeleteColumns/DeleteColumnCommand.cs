using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Columns.DeleteColumn
{
    // IRequest<Unit> หมายถึง Command นี้ไม่คืนค่าอะไรเมื่อสำเร็จ
    public class DeleteColumnCommand : IRequest<Unit>
    {
        [Required]
        public int ColumnId { get; set; } // ID ของ Column ที่จะลบ
    }
}
