using AutoMapper;
using MediatR;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces;
using ProjectEntity = ProjectHub.Domain.Entities.Projects;

namespace ProjectHub.Application.Features.Projects.CreateProject
{
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectResponseDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateProjectHandler(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ProjectResponseDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var isDuplicate = await _projectRepository.IsProjectNameUniqueForUserAsync(request.UserId, request.Name);
            if (isDuplicate)
                throw new ArgumentException($"Project name '{request.Name}' is already in use by this user.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Project name is required.");

            // Map จาก Command -> Entity (ใช้ alias ป้องกัน namespace clash)
            var entity = _mapper.Map<ProjectEntity>(request);

            await _projectRepository.AddProjectAsync(entity);

            // Map จาก Entity -> DTO
            return _mapper.Map<ProjectResponseDto>(entity);
        }
    }
}
