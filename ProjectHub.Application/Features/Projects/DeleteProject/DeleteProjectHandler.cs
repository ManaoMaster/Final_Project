using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Interfaces; 
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Projects.DeleteProject 
{
    
    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectSecurityService _securityService; 

        public DeleteProjectHandler(
            IProjectRepository projectRepository, 
            IProjectSecurityService securityService) 
        {
            _projectRepository = projectRepository;
            _securityService = securityService; 
        }

        public async Task<Unit> Handle(
            DeleteProjectCommand request,
            CancellationToken cancellationToken
        )
        {
            
            
            
            await _securityService.ValidateProjectAccessAsync(request.ProjectId);

            
            await _projectRepository.DeleteProjectAsync(request.ProjectId);

            
            return Unit.Value;
        }
    }
}