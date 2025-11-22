using System.Linq; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities; 
using ProjectHub.Infrastructure.Persistence; 

namespace ProjectHub.Infrastructure.Repositories
{
    
    public class ColumnRepository : IColumnRepository
    {
        private readonly AppDbContext _context;

        public ColumnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsColumnNameUniqueForTableAsync(int tableId, string columnName)
        {
            
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
            
            return await _context.Columns.AnyAsync(c =>
                c.Table_id == tableId && c.Is_primary == true
            );
        }

        public async Task AddColumnAsync(Columns column)
        {
            
            await _context.Columns.AddAsync(column);
            
            await _context.SaveChangesAsync();
        }

        
        public async Task<Columns?> GetColumnByIdAsync(int columnId)
        {
            return await _context.Columns.FindAsync(columnId);
        }

        
        
        public async Task<IEnumerable<Columns>> GetColumnsByTableIdAsync(int tableId)
        {
            
            
            
            return await _context.Columns
        .Include(c => c.LookupRelationship)
            .ThenInclude(r => r.PrimaryColumn)
        .Where(c => c.Table_id == tableId)
        .ToListAsync();
        }

        public async Task UpdateColumnAsync(Columns column)
        {
            _context.Columns.Update(column);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColumnAsync(int columnId)
        {
            
            var columnToDelete = await _context.Columns.FindAsync(columnId);

            
            if (columnToDelete != null)
            {
                _context.Columns.Remove(columnToDelete);
                await _context.SaveChangesAsync();
                
            }
            
        }
        public async Task<IEnumerable<Columns>> GetByIdsAsync(List<int> ids)
        {
            
            return await _context.Columns.Where(c => ids.Contains(c.Column_id)).ToListAsync();

            
            
            
        }
        public async Task<Columns> CreateColumnWithNewRelationshipAsync(
    Columns columnEntity,
    Relationships newRelationship)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                _context.Columns.Add(columnEntity);
                await _context.SaveChangesAsync();   

                
                
                newRelationship.ForeignTableId = columnEntity.Table_id;   
                newRelationship.ForeignColumnId = columnEntity.Column_id;

                _context.Relationships.Add(newRelationship);
                await _context.SaveChangesAsync();   

                
                columnEntity.LookupRelationshipId = newRelationship.RelationshipId;
                _context.Columns.Update(columnEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return columnEntity;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    }
