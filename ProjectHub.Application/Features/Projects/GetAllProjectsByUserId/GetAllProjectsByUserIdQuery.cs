using MediatR;
using ProjectHub.Application.Dtos;
using System.Collections.Generic;

namespace ProjectHub.Application.Features.Projects.GetAllProjects
{
    public class GetAllProjectsQuery : IRequest<IEnumerable<ProjectResponseDto>>
    {
        
        
        
        public int UserId { get; set; }
    }
}