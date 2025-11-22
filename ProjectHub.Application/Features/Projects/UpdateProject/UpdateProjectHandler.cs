using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Projects.UpdateProject
{
    public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        private readonly IProjectSecurityService _securityService;

        public UpdateProjectHandler(IProjectRepository projectRepository, IMapper mapper, IProjectSecurityService securityService)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
            _securityService = securityService;
        }


        public async Task<ProjectResponseDto> Handle(
            UpdateProjectCommand request,
            CancellationToken cancellationToken
        )
        {
            
            var projectToUpdate = await _projectRepository.GetProjectByIdAsync(request.ProjectId);

            
            if (projectToUpdate == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
                
            }


            await _securityService.ValidateProjectAccessAsync(request.ProjectId);
            
            
            
            

            
            _mapper.Map(request, projectToUpdate);

            
            await _projectRepository.UpdateProjectAsync(projectToUpdate);

            
            var responseDto = _mapper.Map<ProjectResponseDto>(projectToUpdate);

            return responseDto;
        }
    }
}
