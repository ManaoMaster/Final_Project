using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    // IRequest<Unit> หมายถึง Command นี้ไม่คืนค่าอะไรเมื่อสำเร็จ
    public class DeleteProjectCommand : IRequest<Unit>
    {
        [Required]
        public int ProjectId { get; set; } // ID ของ Project ที่จะลบ
    }
}
