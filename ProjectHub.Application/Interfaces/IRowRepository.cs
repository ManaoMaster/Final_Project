using System.Threading.Tasks;
using ProjectHub.Domain.Entities; // ใช้ Entity Rows

namespace ProjectHub.Application.Repositories
{
    // สัญญาสำหรับการจัดการข้อมูลแถว (Row)
    public interface IRowRepository
    {
        // สัญญาข้อที่ 1: เพิ่มแถวใหม่ลงใน Database
        // Input: Object Rows ที่สร้างเสร็จแล้ว (จาก Handler)
        // Output: ไม่มี (Task เฉยๆ)
        Task AddRowAsync(Rows row);
        Task<Rows?> GetRowByIdAsync(int rowId);

        Task UpdateRowAsync(Rows row);
        // --- (เมธอดอื่นๆ ที่อาจจะเพิ่มในอนาคต) ---

        Task DeleteRowAsync(int rowId);

        Task<int> GetMaxPkValueAsync(int tableId, string pkColumnName);

        Task<bool> IsPkValueDuplicateAsync(int tableId, string pkColumnName, string pkValue);
    }
}
