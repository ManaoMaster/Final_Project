using MediatR;
using ProjectHub.Application.Dtos; 

namespace ProjectHub.Application.Features.Projects.ToggleFavoriteProject
{
    
    
    
    public class ToggleFavoriteProjectCommand : IRequest<ProjectResponseDto>
    {
        
        public int ProjectId { get; set; }

        
        public int UserId { get; set; }
    }
}