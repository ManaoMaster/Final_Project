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

        // --- เพิ่ม Implementation ของ GetTableByIdAsync ---
        // Implement การดึง Table ตาม ID (สำหรับ Handler)
        public async Task<Tables?> GetTableByIdAsync(int tableId)
        {
            // ใช้ FindAsync ซึ่งเป็นวิธีที่เร็วที่สุดในการหา Entity ตาม Primary Key
            // คืนค่า Table object หรือ null ถ้าไม่เจอ ID นั้น
            return await _context.Tables.FindAsync(tableId);
        }
    }
}
