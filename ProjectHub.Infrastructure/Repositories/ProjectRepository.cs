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
            .Include(p => p.Tables) // <-- [!] ผมแอบเปิด .Include() ให้นะครับ
            .FirstOrDefaultAsync(p => p.Project_id == projectId);
        }

        public async Task UpdateProjectAsync(Projects project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            // 1. ค้นหา Row ด้วย ID
            var projectToDelete = await _context.Projects.FindAsync(projectId);

            // 2. ถ้าเจอ ให้สั่งลบ
            if (projectToDelete != null)
            {
                _context.Projects.Remove(projectToDelete);
                await _context.SaveChangesAsync();
                // ไม่จำเป็นต้องกังวลเรื่อง Cascade Delete ที่นี่ เพราะ Row ไม่มีข้อมูลลูก
            }
            // ถ้าไม่เจอ ก็ไม่ต้องทำอะไร (Handler ควรจะเช็คเจอไปก่อนแล้ว)
        }

        public async Task<IEnumerable<Projects>> GetProjectsByUserIdAsync(int userId)
        {
            // --- *** [FIX 1] *** ---
            // เราต้อง .Include(p => p.Tables) ที่นี่ด้วย
            // เพื่อให้ AutoMapper ใน Step 6.2 สามารถนับ .Tables.Count ได้
            return await _context.Projects
                   .Where(p => p.User_id == userId)
                           .Include(p => p.Tables) // <-- *** เพิ่มบรรทัดนี้ ***
            .AsNoTracking()
                   .ToListAsync();
        }
        public async Task UpdateTimestampsAsync(Projects project)
        {
            // 1. "แนบ" (Attach) Entity เข้ากับ Context
            // (บอก EF Core ว่า "ช่วย Track ตัวนี้หน่อย แต่ไม่ต้องทำอะไร")
            _context.Projects.Attach(project);

            // 2. "สั่ง" (Mark) ว่าเราต้องการอัปเดต *เฉพาะ* Property นี้เท่านั้น
            _context.Entry(project).Property(p => p.LastOpenedAt).IsModified = true;

            // (เราจะไม่แตะ UpdatedAt หรือ IsFavorite)

            // 3. บันทึก (EF Core จะรัน SQL UPDATE ที่อัปเดตแค่ 1 Field)
            await _context.SaveChangesAsync();
        }

    }
}
