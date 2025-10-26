using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; // ใช้ Entity Tables
using ProjectHub.Infrastructure.Persistence; // ใช้ DbContext
using System.Threading.Tasks;

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
            return await _context.Tables
                .AnyAsync(t => t.Project_id == projectId && t.Name == tableName);
        }

        // Implement การเพิ่ม Table ใหม่
        public async Task AddTableAsync(Tables table)
        {
            await _context.Tables.AddAsync(table);
            await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงลง DB
        }
    }
}
