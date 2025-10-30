using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.DTOs;

namespace ProjectHub.Application.Features.Relationships.CreateRelationship
{
    // Command สำหรับสร้าง "กฎ" ความสัมพันธ์
    // คืนค่าเป็น DTO ที่เพิ่งสร้าง
    public class CreateRelationshipCommand : IRequest<RelationshipResponseDto>
    {
        [Required]
        public int PrimaryTableId { get; set; }

        [Required]
        public int PrimaryColumnId { get; set; }

        [Required]
        public int ForeignTableId { get; set; }

        [Required]
        public int ForeignColumnId { get; set; }
    }
}
