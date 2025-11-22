using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; 
using ProjectHub.Infrastructure.Persistence; 

namespace ProjectHub.Infrastructure.Repositories
{
    
    public class TableRepository : ITableRepository
    {
        private readonly AppDbContext _context;

        
        public TableRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<bool> IsTableNameUniqueForProjectAsync(int projectId, string tableName)
        {
            return await _context.Tables.AnyAsync(t =>
                t.Project_id == projectId && t.Name == tableName
            );
        }

        
        public async Task AddTableAsync(Tables table)
        {
            await _context.Tables.AddAsync(table);
            await _context.SaveChangesAsync(); 
        }

        public async Task<Tables?> GetTableByIdAsync(int tableId)
        {
            
            return await _context.Tables.FindAsync(tableId);
        }

        
        public async Task UpdateTableAsync(Tables tableToUpdate)
        {
            
            
            
            await _context.SaveChangesAsync();

            
            
            
        }

        public async Task DeleteTableAsync(int tableId)
        {
            
            var tableToDelete = await _context.Tables.FindAsync(tableId);

            
            if (tableToDelete != null)
            {
                _context.Tables.Remove(tableToDelete);
                await _context.SaveChangesAsync();
                
            }
            
        }

        public async Task<IEnumerable<Tables>> GetTablesByProjectIdAsync(int projectId)
        {
            return await _context.Tables
                .Where(t => t.Project_id == projectId)
                .AsNoTracking() 
                .ToListAsync();
        }
    }
}
