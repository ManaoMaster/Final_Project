using System.Threading.Tasks;
using ProjectHub.Application.Repositories;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace ProjectHub.Infrastructure.Repositories
{
    
    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly AppDbContext _context;

        
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
            
            _context.Entry(relationship).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Relationships relationship)
        {
            _context.Relationships.Remove(relationship);
            await _context.SaveChangesAsync();
        }

        
        public async Task AddRelationshipAsync(Relationships relationship)
        {
            
            await _context.Relationships.AddAsync(relationship);

            
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Relationships>> GetByIdsAsync(List<int> ids)
        {
            
            return await _context.Relationships.Where(r => ids.Contains(r.RelationshipId)).ToListAsync();

            
            
            
        }
        
    }
}
