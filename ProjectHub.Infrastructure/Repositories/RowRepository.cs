using ProjectHub.Application.Repositories; // ใช้ Interface จาก Application
using ProjectHub.Domain.Entities;         // ใช้ Entity Rows
using ProjectHub.Infrastructure.Persistence; // ใช้ AppDbContext
using System.Threading.Tasks;             // ใช้ Task

namespace ProjectHub.Infrastructure.Repositories
{
    // Class นี้ต้อง Implement IRowRepository
    public class RowRepository : IRowRepository
    {
        private readonly AppDbContext _context; // Inject DbContext

        public RowRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implement เมธอด AddRowAsync ตามสัญญา
        public async Task AddRowAsync(Rows row)
        {
            // ใช้ EF Core เพื่อเพิ่ม Entity Rows ใหม่เข้า DbContext
            await _context.Rows.AddAsync(row);
            // บันทึกการเปลี่ยนแปลงลง Database จริง
            await _context.SaveChangesAsync();
        }

        // --- (Implement เมธอดอื่นๆ ของ IRowRepository ที่อาจมีในอนาคต) ---
        // public async Task<IEnumerable<Rows>> GetRowsByTableIdAsync(int tableId) { ... }
        // public async Task<Rows?> GetRowByIdAsync(int rowId) { ... }
        // public async Task UpdateRowAsync(Rows row) { ... }
        // public async Task DeleteRowAsync(int rowId) { ... }
    }
}

