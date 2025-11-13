using Microsoft.AspNetCore.Http;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

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
          IProjectRepository projectRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _tableRepository = tableRepository;
            _projectRepository = projectRepository;
        }

        // --- 1. Helper สำหรับดึง User ID ที่ล็อกอิน ---
        public int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?
              .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("ไม่สามารถระบุตัวตนผู้ใช้งานได้");
            }
            return userId;
        }

        // --- 2. Logic ตรวจสอบสิทธิ์ของ Project ---
        public async Task ValidateProjectAccessAsync(int projectId)
        {
            var currentUserId = GetCurrentUserId();

            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project ID {projectId} not found.");

            if (project.User_id != currentUserId)
                throw new UnauthorizedAccessException($"ไม่มีสิทธิ์ใน Project ID: {projectId}");

            // อัปเดต recent-open
            project.LastOpenedAt = DateTime.UtcNow;
            await _projectRepository.UpdateTimestampsAsync(project);
        }


        // --- 3. Logic ตรวจสอบสิทธิ์ของ Table (ที่เราจะใช้บ่อย) ---
        public async Task ValidateTableAccessAsync(int tableId)
        {
            var table = await _tableRepository.GetTableByIdAsync(tableId);

            if (table == null)
            {
                throw new Exception($"ไม่พบ Table ID: {tableId}");
            }

            // ตรวจสอบสิทธิ์โดยการเช็ค Project ที่เป็นเจ้าของ Table นี้
            // (เมื่อ Method นี้ถูกเรียก มันจะไปเรียก ValidateProjectAccessAsync
            // ซึ่งจะ Trigger Logic [FIX] ของเราโดยอัตโนมัติ)
            await ValidateProjectAccessAsync(table.Project_id);
        }
    }
}