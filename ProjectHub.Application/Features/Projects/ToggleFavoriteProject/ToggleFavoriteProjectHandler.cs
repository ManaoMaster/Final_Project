using MediatR;
using AutoMapper;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Interfaces; // (สำหรับ IProjectRepository)
using ProjectHub.Application.Repositories; // (ถ้า Interface อยู่คนละที่)
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
            // 1. (Security) ดึง Project และตรวจสอบสิทธิ์เจ้าของ
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);

            if (project == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
            }
            if (project.User_id != request.UserId)
            {
                // User พยายามกดดาว Project ของคนอื่น
                throw new UnauthorizedAccessException("You do not have access to this project.");
            }

            // 2. --- [Logic หลัก] ---
            // พลิกค่า (Toggle)
            project.IsFavorite = !project.IsFavorite;

            // อัปเดต "Last modified"
            project.UpdatedAt = DateTime.UtcNow;

            // 3. บันทึกการเปลี่ยนแปลง
            // (เราใช้ UpdateProjectAsync ที่คุณมีอยู่แล้ว)
            await _projectRepository.UpdateProjectAsync(project);

            // 4. Map Entity -> DTO และส่งผลลัพธ์ใหม่กลับไป
            // (เราต้อง Map TableCount เอง เพราะ AutoMapper ไม่รู้จัก)
            var responseDto = _mapper.Map<ProjectResponseDto>(project);
            responseDto.TableCount = project.Tables.Count; // (นับจำนวน Table ที่โหลดมา)

            return responseDto;
        }
    }
}