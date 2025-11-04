using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Repositories
{
    // สัญญาสำหรับการจัดการข้อมูล "กฎ" ความสัมพันธ์
    public interface IRelationshipRepository
    {
        Task AddRelationshipAsync(Relationships relationship);

        Task UpdateAsync(Relationships relationship);

        Task DeleteAsync(Relationships relationship);

        Task<Relationships?> GetByIdAsync(int id);
        Task<IEnumerable<Relationships>> GetByIdsAsync(List<int> ids);
    }
}
