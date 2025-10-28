using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories
{
    // นี่คือ "การทำงานจริง" (Implementation)
    public class ProjectRepository : IProjectRepository
    {
        // 1. Inject DbContext (เครื่องมือคุยกับ DB)
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        // 2. Implement การบันทึก Project
        public async Task AddProjectAsync(Projects project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        // 3. Implement การตรวจสอบชื่อซ้ำ (Logic ที่คุณต้องการ)
        //    "ตรวจสอบว่าชื่อโปรเจกต์นี้ซ้ำหรือไม่ *สำหรับ User คนนี้*"
        public async Task<bool> IsProjectNameUniqueForUserAsync(int userId, string projectName)
        {
            // ใช้ EF Core ค้นหาในตาราง Projects
            // โดยมีเงื่อนไขว่า User_id ต้องตรงกัน และ Name ต้องตรงกัน
            return await _context.Projects.AnyAsync(p =>
                p.User_id == userId && p.Name == projectName
            );
        }

        public async Task<Projects?> GetProjectByIdAsync(int projectId)
        {
            // ใช้ FirstOrDefaultAsync เพื่อหา Project ตาม ID
            // Include(p => p.Tables) ถ้าต้องการโหลด Tables มาด้วย (Optional)
            return await _context.Projects
            // .Include(p => p.Tables)
            .FirstOrDefaultAsync(p => p.Project_id == projectId);
        }

        public async Task UpdateProjectAsync(Projects project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
