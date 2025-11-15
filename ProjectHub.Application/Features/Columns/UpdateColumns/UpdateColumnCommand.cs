using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos; // ใช้ DTO เดิม
using System.Text.Json.Serialization;

// Namespace อาจจะต้องเปลี่ยนตาม Folder ใหม่ (ถ้าคุณเปลี่ยนชื่อ Folder)
namespace ProjectHub.Application.Features.Columns.UpdateColumn
{
    // เปลี่ยนชื่อ Class จาก Edit... เป็น Update...
    public class UpdateColumnCommand : IRequest<ColumnResponseDto>
    {
        [Required]
        public int ColumnId { get; set; } // ID ของ Column ที่จะแก้ไข

        [Required]
        [MaxLength(100)]
        [JsonPropertyName("columnName")]
        public string NewName { get; set; } = string.Empty; // ชื่อใหม่ (ยังใช้ NewName ได้ หรือจะเปลี่ยนเป็น Name ก็ได้)

        [Required]
        [JsonPropertyName("dataType")]
        public string NewDataType { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("isPrimary")]
        public bool NewIsPrimary { get; set; }

        [Required]
        [JsonPropertyName("isNullable")]
        public bool NewIsNullable { get; set; }
    }
}
