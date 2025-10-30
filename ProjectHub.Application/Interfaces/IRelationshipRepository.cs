using ProjectHub.Domain.Entities;
using System.Threading.Tasks;

namespace ProjectHub.Application.Repositories
{
    // สัญญาสำหรับการจัดการข้อมูล "กฎ" ความสัมพันธ์
    public interface IRelationshipRepository
    {
        // เพิ่ม "กฎ" ความสัมพันธ์ใหม่
        Task AddRelationshipAsync(Relationships relationship);

        // (Optional) อาจจะเพิ่มเมธอดเช็คว่าความสัมพันธ์นี้ซ้ำซ้อนหรือไม่
        // Task<bool> DoesRelationshipExistAsync(int primaryColumnId, int foreignColumnId);
    }
}
