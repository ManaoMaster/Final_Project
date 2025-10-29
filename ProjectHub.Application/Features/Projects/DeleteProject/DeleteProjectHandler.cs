using System; // For ArgumentException
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    // Handler for DeleteProjectCommand, returns Unit (nothing)
    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
    {
        private readonly IProjectRepository _projectRepository;

        public DeleteProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<Unit> Handle(
            DeleteProjectCommand request,
            CancellationToken cancellationToken
        )
        {
            // Optional but recommended: Check if the project exists before deleting
            // This provides a clearer error message than letting the repository handle it silently
            var projectExists = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (projectExists == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
                // Or use a custom NotFoundException
            }

            // Call the repository to delete the project by ID
            await _projectRepository.DeleteProjectAsync(request.ProjectId);

            // Return Unit.Value to indicate success with no return data
            return Unit.Value;
        }
    }
}
