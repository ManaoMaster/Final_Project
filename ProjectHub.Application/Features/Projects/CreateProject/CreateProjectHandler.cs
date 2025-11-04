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
        private readonly IProjectSecurityService _securityService;

        public CreateProjectHandler(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IProjectSecurityService securityService
            )
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<ProjectResponseDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _securityService.GetCurrentUserId();

            // เช็คว่า User ที่ล็อกอิน (currentUserId)
            // พยายามสร้างโปรเจกต์ให้ตัวเอง (request.UserId) ใช่หรือไม่
            if (request.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("คุณสามารถสร้างโปรเจกต์ให้ตัวเองได้เท่านั้น");
            }
            
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
