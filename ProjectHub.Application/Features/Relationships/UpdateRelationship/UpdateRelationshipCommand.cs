using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Relationships.UpdateRelationship
{
    public class UpdateRelationshipCommand : IRequest
    {
        public int Id { get; set; } // ID ของ Relationship ที่จะแก้

        // ข้อมูล Metadata ที่อนุญาตให้แก้ได้
        [Required]
        public string? DisplayName { get; set; }
        public string? Notes { get; set; }
    }
}
