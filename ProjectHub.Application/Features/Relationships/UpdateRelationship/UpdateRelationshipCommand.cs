using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Relationships.UpdateRelationship
{
    public class UpdateRelationshipCommand : IRequest
    {
        public int Id { get; set; } 

        
        [Required]
        public string? DisplayName { get; set; }
        public string? Notes { get; set; }
    }
}
