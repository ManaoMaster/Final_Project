using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Features.Projects.DeleteProject;
using ProjectHub.Application.Interfaces; // 1. [ADD] Import "Yara"
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Projects.DeleteProject // (Fixed Namespace)
{
    // Handler for DeleteProjectCommand, returns Unit (nothing)
    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectSecurityService _securityService; // 2. [ADD] Inject "Yara"

        public DeleteProjectHandler(
            IProjectRepository projectRepository, 
            IProjectSecurityService securityService) // 3. [ADD] Receive "Yara"
        {
            _projectRepository = projectRepository;
            _securityService = securityService; // 4. [ADD] Assign "Yara"
        }

        public async Task<Unit> Handle(
            DeleteProjectCommand request,
            CancellationToken cancellationToken
        )
        {
            // 5. [OPTIMIZE] Call "Yara" FIRST.
            // This single line handles both checking if it exists AND if you have access.
            // It will throw an Exception if not found or not authorized.
            await _securityService.ValidateProjectAccessAsync(request.ProjectId);

            // 6. (If OK) Call the repository to delete the project by ID
            await _projectRepository.DeleteProjectAsync(request.ProjectId);

            // 7. Return Unit.Value to indicate success
            return Unit.Value;
        }
    }
}