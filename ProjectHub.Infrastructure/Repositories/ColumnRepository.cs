using System.Linq; // เพิ่ม using นี้สำหรับ .AnyAsync()
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // เพิ่ม using นี้สำหรับ EF Core
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; // ใช้ Entity Columns
using ProjectHub.Infrastructure.Persistence; // ใช้ DbContext

namespace ProjectHub.Infrastructure.Repositories
{
    // ต้อง Implement IColumnRepository
    public class ColumnRepository : IColumnRepository
    {
        private readonly AppDbContext _context;

        public ColumnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsColumnNameUniqueForTableAsync(int tableId, string columnName)
        {
            // ใช้ EF Core query กับ DbContext เพื่อเช็คชื่อซ้ำใน Table เดียวกัน
            return await _context.Columns.AnyAsync(c =>
                c.Table_id == tableId && c.Name == columnName
            );
        }
        public async Task<Columns?> GetColumnByNameAsync(int tableId, string name)
        {
            return await _context.Columns
                .FirstOrDefaultAsync(c => c.Table_id == tableId && c.Name == name);
        }
        public async Task<bool> HasPrimaryKeyAsync(int tableId)
        {
            // ใช้ EF Core query เพื่อเช็คว่ามี Column ไหนใน Table นี้ที่เป็น Primary Key แล้ว
            return await _context.Columns.AnyAsync(c =>
                c.Table_id == tableId && c.Is_primary == true
            );
        }

        public async Task AddColumnAsync(Columns column)
        {
            // เพิ่ม Column ใหม่เข้า DbContext
            await _context.Columns.AddAsync(column);
            // บันทึกการเปลี่ยนแปลงลง Database
            await _context.SaveChangesAsync();
        }

        // GetColumnByIdAsync (อาจจะไม่จำเป็นสำหรับ Create แต่ใส่ไว้เผื่ออนาคต)
        public async Task<Columns?> GetColumnByIdAsync(int columnId)
        {
            return await _context.Columns.FindAsync(columnId);
        }

        // --- เพิ่ม Implementation ของ GetColumnsByTableIdAsync ---
        // Implement การดึง Columns ทั้งหมดของ Table (สำหรับ Validate JSON ใน CreateRowHandler)
        public async Task<IEnumerable<Columns>> GetColumnsByTableIdAsync(int tableId)
        {
            // ใช้ Where เพื่อกรอง Columns ที่มี Table_id ตรงกัน
            // ToListAsync เพื่อดึงข้อมูลทั้งหมดมาเป็น List ใน Memory
            return await _context.Columns.Where(c => c.Table_id == tableId).ToListAsync();
        }

        public async Task UpdateColumnAsync(Columns column)
        {
            _context.Columns.Update(column);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColumnAsync(int columnId)
        {
            // 1. ค้นหา Row ด้วย ID
            var columnToDelete = await _context.Columns.FindAsync(columnId);

            // 2. ถ้าเจอ ให้สั่งลบ
            if (columnToDelete != null)
            {
                _context.Columns.Remove(columnToDelete);
                await _context.SaveChangesAsync();
                // ไม่จำเป็นต้องกังวลเรื่อง Cascade Delete ที่นี่ เพราะ Row ไม่มีข้อมูลลูก
            }
            // ถ้าไม่เจอ ก็ไม่ต้องทำอะไร (Handler ควรจะเช็คเจอไปก่อนแล้ว)
        }
        public async Task<IEnumerable<Columns>> GetByIdsAsync(List<int> ids)
        {
            // (ถ้าใช้ EF Core)
            return await _context.Columns.Where(c => ids.Contains(c.Column_id)).ToListAsync();

            // (ถ้าใช้ Dapper)
            // var sql = "SELECT * FROM \"Columns\" WHERE \"Id\" = ANY(@ids)";
            // return await _dbConnection.QueryAsync<Columns>(sql, new { ids });
        }
    }
}
