using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectHub.Application.Features.Projects.ToggleFavoriteProject
{
    public class ToggleFavoriteProjectHandler : IRequestHandler<ToggleFavoriteProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ToggleFavoriteProjectHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ProjectResponseDto> Handle(ToggleFavoriteProjectCommand request, CancellationToken cancellationToken)
        {
            
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);

            if (project == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
            }
            if (project.User_id != request.UserId)
            {
                
                throw new UnauthorizedAccessException("You do not have access to this project.");
            }

            
            
            project.IsFavorite = !project.IsFavorite;

            
            project.UpdatedAt = DateTime.UtcNow;

            
            
            await _projectRepository.UpdateProjectAsync(project);

            
            
            var responseDto = _mapper.Map<ProjectResponseDto>(project);
            responseDto.TableCount = project.Tables.Count; 

            return responseDto;
        }
    }
}