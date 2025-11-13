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
            // 1. ดึงข้อมูล Project เดิมจาก ID
            var projectToUpdate = await _projectRepository.GetProjectByIdAsync(request.ProjectId);

            // 2. ตรวจสอบว่าเจอหรือไม่
            if (projectToUpdate == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
                // หรืออาจจะใช้ Exception ที่ Custom ขึ้นมาเอง เช่น NotFoundException
            }


            await _securityService.ValidateProjectAccessAsync(request.ProjectId);
            // (Optional) ตรวจสอบ Business Rule เพิ่มเติม
            // เช่น เช็คว่าชื่อใหม่ซ้ำหรือไม่ (ถ้าต้องการ) โดยเรียก IsProjectNameUniqueForUserAsync
            // var isDuplicate = await _projectRepository.IsProjectNameUniqueForUserAsync(projectToUpdate.User_id, request.NewName);
            // if (isDuplicate) { throw new ArgumentException(...); }

            // 3. ใช้ AutoMapper เพื่ออัปเดตเฉพาะ Field ที่ Map ไว้ (คือ Name)
            _mapper.Map(request, projectToUpdate);

            // 4. เรียก Repository เพื่อบันทึกการเปลี่ยนแปลง
            await _projectRepository.UpdateProjectAsync(projectToUpdate);

            // 5. Map Entity ที่อัปเดตแล้ว กลับเป็น DTO เพื่อส่งคืน
            var responseDto = _mapper.Map<ProjectResponseDto>(projectToUpdate);

            return responseDto;
        }
    }
}
