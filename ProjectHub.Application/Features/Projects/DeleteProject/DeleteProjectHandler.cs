using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
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
            // 1. ดึงข้อมูล Project ที่ต้องการลบ
            var projectToDelete = await _projectRepository.GetProjectByIdAsync(request.ProjectId);

            // 2. ตรวจสอบว่าเจอหรือไม่
            if (projectToDelete == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
                // หรือใช้ NotFoundException
            }

            // (Optional) ตรวจสอบ Business Rule อื่นๆ ก่อนลบได้
            // เช่น เช็คว่า User ที่ขอลบ เป็นเจ้าของ Project หรือไม่ (ถ้ามีระบบ Auth)

            // 3. เรียก Repository เพื่อทำการลบ
            await _projectRepository.DeleteProjectAsync(projectToDelete);

            // 4. คืนค่า Unit.Value เพื่อบอกว่าสำเร็จ (ไม่มีข้อมูลส่งกลับ)
            return Unit.Value;
        }
    }
}
