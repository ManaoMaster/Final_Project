using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; // ใช้ Entity Tables
using ProjectHub.Infrastructure.Persistence; // ใช้ DbContext

namespace ProjectHub.Infrastructure.Repositories
{
    // Class นี้ Implement สัญญาจาก Application Layer
    public class TableRepository : ITableRepository
    {
        private readonly AppDbContext _context;

        // Inject DbContext
        public TableRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implement การตรวจสอบชื่อซ้ำ (เฉพาะใน Project)
        public async Task<bool> IsTableNameUniqueForProjectAsync(int projectId, string tableName)
        {
            return await _context.Tables.AnyAsync(t =>
                t.Project_id == projectId && t.Name == tableName
            );
        }

        // Implement การเพิ่ม Table ใหม่
        public async Task AddTableAsync(Tables table)
        {
            await _context.Tables.AddAsync(table);
            await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงลง DB
        }

        public async Task<Tables?> GetTableByIdAsync(int tableId)
        {
            // ใช้ FindAsync ซึ่งเป็นวิธีที่เร็วที่สุดในการหา Entity ตาม Primary Key
            return await _context.Tables.FindAsync(tableId);
        }

        // *** เพิ่ม: Implementation ของ UpdateTableAsync ***
        public async Task UpdateTableAsync(Tables tableToUpdate)
        {
            // เนื่องจาก tableToUpdate ถูกดึงมาจาก Context (ผ่าน GetTableByIdAsync)
            // EF Core จะ Track การเปลี่ยนแปลง Property (เช่น Name) โดยอัตโนมัติ
            // เราแค่สั่ง SaveChanges() ก็พอ
            await _context.SaveChangesAsync();

            // หรือถ้าต้องการความชัดเจน (เผื่อ Entity ไม่ได้ถูก Track)
            // _context.Tables.Update(tableToUpdate);
            // await _context.SaveChangesAsync();
        }

        public async Task DeleteTableAsync(int tableId)
        {
            // 1. ค้นหา Row ด้วย ID
            var tableToDelete = await _context.Tables.FindAsync(tableId);

            // 2. ถ้าเจอ ให้สั่งลบ
            if (tableToDelete != null)
            {
                _context.Tables.Remove(tableToDelete);
                await _context.SaveChangesAsync();
                // ไม่จำเป็นต้องกังวลเรื่อง Cascade Delete ที่นี่ เพราะ Row ไม่มีข้อมูลลูก
            }
            // ถ้าไม่เจอ ก็ไม่ต้องทำอะไร (Handler ควรจะเช็คเจอไปก่อนแล้ว)
        }

        public async Task<IEnumerable<Tables>> GetTablesByProjectIdAsync(int projectId)
        {
            return await _context.Tables
                .Where(t => t.Project_id == projectId)
                .AsNoTracking() // (แนะนำให้ใช้ .AsNoTracking() เพื่อให้ Read-Only query เร็วขึ้น)
                .ToListAsync();
        }
    }
}
