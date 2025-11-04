using System.Threading.Tasks; // ใช้ Task
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Repositories; // ใช้ Interface จาก Application
using ProjectHub.Domain.Entities; // ใช้ Entity Rows
using ProjectHub.Infrastructure.Persistence; // ใช้ AppDbContext
using Dapper; // ใช้ Dapper สำหรับรัน SQL แบบ Raw


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

        public async Task UpdateRowAsync(Rows row)
        {
            // ใช้ EF Core เพื่ออัปเดต Entity Rows ใน DbContext
            _context.Rows.Update(row);
            // บันทึกการเปลี่ยนแปลงลง Database จริง
            await _context.SaveChangesAsync();
        }

        public async Task<Rows?> GetRowByIdAsync(int rowId)
        {
            // ใช้ FindAsync ซึ่งเป็นวิธีที่เร็วที่สุดในการหา Entity ตาม Primary Key
            return await _context.Rows.FindAsync(rowId);
        }

        public async Task DeleteRowAsync(int rowId)
        {
            // 1. ค้นหา Row ด้วย ID
            var rowToDelete = await _context.Rows.FindAsync(rowId);

            // 2. ถ้าเจอ ให้สั่งลบ
            if (rowToDelete != null)
            {
                _context.Rows.Remove(rowToDelete);
                await _context.SaveChangesAsync();
                // ไม่จำเป็นต้องกังวลเรื่อง Cascade Delete ที่นี่ เพราะ Row ไม่มีข้อมูลลูก
            }
            // ถ้าไม่เจอ ก็ไม่ต้องทำอะไร (Handler ควรจะเช็คเจอไปก่อนแล้ว)
        }
        public async Task<int> GetMaxPkValueAsync(int tableId, string pkColumnName)
        {
            // SQL นี้จะหาค่า "สูงสุด" (MAX) ของคอลัมน์ PK นั้น
            // (COALESCE(..., 0) หมายความว่า ถ้าตารางว่างเปล่า ให้คืนค่า 0)
            var sql = $@"
                SELECT COALESCE(MAX(CAST(""Data""->>@pkColumnName AS INTEGER)), 0) 
                FROM ""Rows"" 
                WHERE ""Table_id"" = @tableId";

            // Dapper จะรันคำสั่งนี้ และส่งค่ากลับมาเป็น int
            var maxId = await _context.Database.GetDbConnection().QuerySingleAsync<int>(sql, new
            {
                pkColumnName, // Dapper จะ Map ค่านี้ให้ @pkColumnName
                tableId       // Dapper จะ Map ค่านี้ให้ @tableId
            });

            return maxId;
        }
        public async Task<bool> IsPkValueDuplicateAsync(int tableId, string pkColumnName, string pkValue)
        {
            // SQL นี้จะ "นับ" (COUNT) ว่ามีแถวที่ใช้ค่า PK นี้แล้วกี่แถว
            var sql = $@"
                SELECT COUNT(1) 
                FROM ""Rows"" 
                WHERE ""Table_id"" = @tableId AND ""Data""->>@pkColumnName = @pkValue";

            // Dapper จะรันคำสั่งนี้ และส่งค่ากลับมาเป็น int
            var count = await _context.Database.GetDbConnection().QuerySingleAsync<int>(sql, new
            {
                tableId,
                pkColumnName,
                pkValue
            });

            // ถ้า count > 0 (เช่น 1) แปลว่า "ซ้ำ" (Duplicate) -> คืนค่า true
            return count > 0;
        }

        // ^^^ --- [สิ้นสุด 2 METHOD ใหม่] --- ^^^
    }
}

