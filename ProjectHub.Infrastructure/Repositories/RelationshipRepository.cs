using System.Threading.Tasks;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Relationships> AddAsync(Relationships relationship)
        {
            await _context.Relationships.AddAsync(relationship);
            await _context.SaveChangesAsync();
            return relationship;
        }

        public async Task<Relationships?> GetByIdAsync(int id)
        {
            return await _context.Relationships.FindAsync(id);
        }

        public async Task UpdateAsync(Relationships relationship)
        {
            // บอก EF Core ว่า Entity นี้มีการแก้ไข
            _context.Entry(relationship).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Relationships relationship)
        {
            _context.Relationships.Remove(relationship);
            await _context.SaveChangesAsync();
        }

        // Implement เมธอด AddRelationshipAsync ตามสัญญา
        public async Task AddRelationshipAsync(Relationships relationship)
        {
            // ใช้ EF Core เพื่อเพิ่ม "กฎ" ใหม่เข้า DbContext
            await _context.Relationships.AddAsync(relationship);

            // บันทึกการเปลี่ยนแปลงลง Database จริง
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Relationships>> GetByIdsAsync(List<int> ids)
        {
            // (ถ้าใช้ EF Core)
            return await _context.Relationships.Where(r => ids.Contains(r.RelationshipId)).ToListAsync();

            // (หรือ Dapper)
            // var sql = "SELECT * FROM \"Relationships\" WHERE \"Id\" = ANY(@ids)";
            // return await _dbConnection.QueryAsync<Relationships>(sql, new { ids });
        }
        // (Implement เมธอดอื่นๆ ของ IRelationshipRepository ที่อาจมีในอนาคต)
    }
}
