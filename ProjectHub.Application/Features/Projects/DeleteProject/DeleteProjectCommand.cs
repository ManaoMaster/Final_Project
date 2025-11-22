using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    
    public class DeleteProjectCommand : IRequest<Unit>
    {
        [Required]
        public int ProjectId { get; set; } 
    }
}
