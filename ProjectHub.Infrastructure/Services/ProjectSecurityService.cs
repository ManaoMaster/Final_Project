using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;

namespace ProjectHub.Infrastructure.Services
{
    public class ProjectSecurityService : IProjectSecurityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITableRepository _tableRepository;
        private readonly IProjectRepository _projectRepository;

        public ProjectSecurityService(
            IHttpContextAccessor httpContextAccessor,
            ITableRepository tableRepository,
            IProjectRepository projectRepository
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _tableRepository = tableRepository;
            _projectRepository = projectRepository;
        }

        
        public int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("ไม่สามารถระบุตัวตนผู้ใช้งานได้");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

            return role == "Admin";
        }

        
        public async Task ValidateProjectAccessAsync(int projectId)
        {
            if (IsAdmin())
            {
                return; 
            }
            var currentUserId = GetCurrentUserId();

            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project ID {projectId} not found.");

            if (project.User_id != currentUserId)
                throw new UnauthorizedAccessException($"ไม่มีสิทธิ์ใน Project ID: {projectId}");

            
            project.LastOpenedAt = DateTime.UtcNow;
            await _projectRepository.UpdateTimestampsAsync(project);
        }

        
        public async Task ValidateTableAccessAsync(int tableId)
        {
            var table = await _tableRepository.GetTableByIdAsync(tableId);

            if (table == null)
            {
                throw new Exception($"ไม่พบ Table ID: {tableId}");
            }

            
            
            
            await ValidateProjectAccessAsync(table.Project_id);
        }
    }
}
