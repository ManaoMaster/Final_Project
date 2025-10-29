using MediatR;
using ProjectHub.Application.Dtos; // ใช้ DTO เดิม
using System.ComponentModel.DataAnnotations;

// Namespace อาจจะต้องเปลี่ยนตาม Folder ใหม่ (ถ้าคุณเปลี่ยนชื่อ Folder)
namespace ProjectHub.Application.Features.Rows.UpdateRow // <-- เปลี่ยน Namespace (ถ้าเปลี่ยน Folder)
{
    // เปลี่ยนชื่อ Class จาก Edit... เป็น Update...
    public class UpdateRowCommand : IRequest<RowResponseDto>
    {
        [Required]
        public int RowId { get; set; } // ID ของ Project ที่จะแก้ไข

        [Required]
        [MaxLength(100)]
        public string NewData { get; set; } = string.Empty; // ชื่อใหม่ (ยังใช้ NewName ได้ หรือจะเปลี่ยนเป็น Name ก็ได้)
    }
}
