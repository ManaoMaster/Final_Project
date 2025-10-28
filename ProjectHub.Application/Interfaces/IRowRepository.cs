using ProjectHub.Domain.Entities; // ใช้ Entity Rows
using System.Threading.Tasks;

namespace ProjectHub.Application.Repositories
{
    // สัญญาสำหรับการจัดการข้อมูลแถว (Row)
    public interface IRowRepository
    {
        // สัญญาข้อที่ 1: เพิ่มแถวใหม่ลงใน Database
        // Input: Object Rows ที่สร้างเสร็จแล้ว (จาก Handler)
        // Output: ไม่มี (Task เฉยๆ)
        Task AddRowAsync(Rows row);

        // --- (เมธอดอื่นๆ ที่อาจจะเพิ่มในอนาคต) ---
        // Task<IEnumerable<Rows>> GetRowsByTableIdAsync(int tableId);
        // Task<Rows?> GetRowByIdAsync(int rowId);
        // Task UpdateRowAsync(Rows row);
        // Task DeleteRowAsync(int rowId);
    }
}

