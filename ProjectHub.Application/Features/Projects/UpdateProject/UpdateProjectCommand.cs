using MediatR;
using ProjectHub.Application.Dtos; 
using System.ComponentModel.DataAnnotations;


namespace ProjectHub.Application.Features.Projects.UpdateProject 
{
    
    public class UpdateProjectCommand : IRequest<ProjectResponseDto>
    {
        [Required]
        public int ProjectId { get; set; } 

        [Required]
        [MaxLength(100)]
        public string NewName { get; set; } = string.Empty; 
    }
}
