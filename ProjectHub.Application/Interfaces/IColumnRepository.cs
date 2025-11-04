using System.Threading.Tasks;
using ProjectHub.Domain.Entities; // ใช้ Entity Columns

namespace ProjectHub.Application.Interfaces // ต้องใช้ Namespace นี้
{
    // นี่คือ Interface (สัญญา) สำหรับจัดการข้อมูล Column
    public interface IColumnRepository
    {
        // สัญญาข้อที่ 1: ตรวจสอบว่าชื่อ Column ซ้ำกันหรือไม่ *ภายใน Table เดียวกัน*
        // Input: ID ของ Table, ชื่อ Column ที่ต้องการตรวจสอบ
        // Output: true ถ้าชื่อซ้ำ, false ถ้าไม่ซ้ำ
        Task<bool> IsColumnNameUniqueForTableAsync(int tableId, string columnName);

        // สัญญาข้อที่ 2: ตรวจสอบว่า Table นี้มี Primary Key อยู่แล้วหรือยัง
        // Input: ID ของ Table ที่ต้องการตรวจสอบ
        // Output: true ถ้ามี PK แล้ว, false ถ้ายังไม่มี
        Task<bool> HasPrimaryKeyAsync(int tableId);

        // สัญญาข้อที่ 3: เพิ่ม Column ใหม่ลงใน Database
        // Input: Object Columns ที่สร้างเสร็จแล้ว (จาก Handler)
        // Output: ไม่มี (Task เฉยๆ)
        Task AddColumnAsync(Columns column); // <--- Parameter name แก้ไขแล้ว

        // สัญญาข้อที่ 4 (Optional): ดึงข้อมูล Column ตาม ID
        // Input: ID ของ Column ที่ต้องการ
        // Output: Object Columns หรือ null ถ้าไม่เจอ
        Task<Columns?> GetColumnByIdAsync(int columnId); // <--- Return type แก้ไขแล้ว

        // สัญญาข้อที่ 5 (Optional): ดึงข้อมูล Column ตาม ID ของ Table
        // Input: ID ของ Table ที่ต้องการ
        // Output: List ของ Object Columns หรือ null ถ้าไม่เจอ
        Task<IEnumerable<Columns>> GetColumnsByTableIdAsync(int tableId);

        Task UpdateColumnAsync(Columns column);

        Task<Columns?> GetColumnByNameAsync(int tableId, string name);
        Task DeleteColumnAsync(int columnId);
        Task<IEnumerable<Columns>> GetByIdsAsync(List<int> ids);

    }
}
