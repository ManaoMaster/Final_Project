using System.Threading.Tasks;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories
{
    // Class นี้ Implement สัญญา IRelationshipRepository
    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly AppDbContext _context;

        // Inject DbContext
        public RelationshipRepository(AppDbContext context)
        {
            _context = context;
        }

        // Implement เมธอด AddRelationshipAsync ตามสัญญา
        public async Task AddRelationshipAsync(Relationships relationship)
        {
            // ใช้ EF Core เพื่อเพิ่ม "กฎ" ใหม่เข้า DbContext
            await _context.Relationships.AddAsync(relationship);

            // บันทึกการเปลี่ยนแปลงลง Database จริง
            await _context.SaveChangesAsync();
        }

        // (Implement เมธอดอื่นๆ ของ IRelationshipRepository ที่อาจมีในอนาคต)
    }
}
