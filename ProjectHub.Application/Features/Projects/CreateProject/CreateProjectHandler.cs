using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Features.Projects.CreateProject
{
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public CreateProjectHandler(
            IProjectRepository projectRepository,
            IUserRepository userRepository
        )
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<ProjectResponseDto> Handle(
            CreateProjectCommand request,
            CancellationToken cancellationToken
        )
        {
            var isDuplicate = await _projectRepository.IsProjectNameUniqueForUserAsync(
                request.UserId,
                request.Name
            );
            if (isDuplicate)
            {
                throw new ArgumentException(
                    $"Project name '{request.Name}' is already in use by this user."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Project name is required.");
            }

            var newProject = new ProjectHub.Domain.Entities.Projects
            {
                User_id = request.UserId,
                Name = request.Name,
            };


            await _projectRepository.AddProjectAsync(newProject);

            var responseDto = new ProjectResponseDto
            {
                ProjectId = newProject.Project_id,
                Name = newProject.Name,
                UserId = newProject.User_id,
                CreatedAt = newProject.Created_at,
            };

            return responseDto;
        }
    }
}
