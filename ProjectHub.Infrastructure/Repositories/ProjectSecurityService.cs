using Microsoft.AspNetCore.Http;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // <-- [FIX] เพิ่ม Using นี้สำหรับ Exception (ถ้ายังไม่มี)

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
            // (ต้องถามเพื่อนคุณว่าใช้ ClaimTypes.NameIdentifier หรืออย่างอื่น)
            var userIdString = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier); 

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                // ถ้าหา UserId ไม่เจอ (เช่น ลืมใส่ Token)
                throw new UnauthorizedAccessException("ไม่สามารถระบุตัวตนผู้ใช้งานได้");
            }
            return userId;
        }

        // --- 2. Logic ตรวจสอบสิทธิ์ของ Project ---
        public async Task ValidateProjectAccessAsync(int projectId)
        {
            var currentUserId = GetCurrentUserId();
            
            // vvv --- [FIX 1] แก้ชื่อ Method ครับ --- vvv
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            // ^^^ --- (จากไฟล์ ProjectRepository.cs ของคุณ) --- ^^^

            if (project == null)
            {
                throw new Exception($"ไม่พบ Project ID: {projectId}");
            }

            // นี่คือหัวใจ: เทียบ ID เจ้าของโปรเจกต์ กับ ID คนที่ล็อกอิน
            if (project.User_id != currentUserId) 
            {
                throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงโปรเจกต์นี้");
            }
        }

        // --- 3. Logic ตรวจสอบสิทธิ์ของ Table (ที่เราจะใช้บ่อย) ---
        public async Task ValidateTableAccessAsync(int tableId)
        {
            // vvv --- [FIX 2] แก้ชื่อ Method ครับ --- vvv
            var table = await _tableRepository.GetTableByIdAsync(tableId);
            // ^^^ --- (จากไฟล์ ITableRepository.cs ของคุณ) --- ^^^

            if (table == null)
            {
                throw new Exception($"ไม่พบ Table ID: {tableId}");
            }

            // ตรวจสอบสิทธิ์โดยการเช็ค Project ที่เป็นเจ้าของ Table นี้
            await ValidateProjectAccessAsync(table.Project_id);
        }
        
    }
}