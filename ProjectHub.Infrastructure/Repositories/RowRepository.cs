using System.Threading.Tasks; 
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Repositories; 
using ProjectHub.Domain.Entities; 
using ProjectHub.Infrastructure.Persistence; 
using Dapper; 


namespace ProjectHub.Infrastructure.Repositories
{
    
    public class RowRepository : IRowRepository
    {
        private readonly AppDbContext _context; 

        public RowRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task AddRowAsync(Rows row)
        {
            
            await _context.Rows.AddAsync(row);
            
            await _context.SaveChangesAsync();
        }

        
        

        public async Task UpdateRowAsync(Rows row)
        {
            
            _context.Rows.Update(row);
            
            await _context.SaveChangesAsync();
        }

        public async Task<Rows?> GetRowByIdAsync(int rowId)
        {
            
            return await _context.Rows.FindAsync(rowId);
        }

        public async Task DeleteRowAsync(int rowId)
        {
            
            var rowToDelete = await _context.Rows.FindAsync(rowId);

            
            if (rowToDelete != null)
            {
                _context.Rows.Remove(rowToDelete);
                await _context.SaveChangesAsync();
                
            }
            
        }
        public async Task<int> GetMaxPkValueAsync(int tableId, string pkColumnName)
        {
            
            
            var sql = $@"
                SELECT COALESCE(MAX(CAST(""Data""->>@pkColumnName AS INTEGER)), 0) 
                FROM ""Rows"" 
                WHERE ""Table_id"" = @tableId";

            
            var maxId = await _context.Database.GetDbConnection().QuerySingleAsync<int>(sql, new
            {
                pkColumnName, 
                tableId       
            });

            return maxId;
        }
        public async Task<bool> IsPkValueDuplicateAsync(int tableId, string pkColumnName, string pkValue)
        {
            
            var sql = $@"
                SELECT COUNT(1) 
                FROM ""Rows"" 
                WHERE ""Table_id"" = @tableId AND ""Data""->>@pkColumnName = @pkValue";

            
            var count = await _context.Database.GetDbConnection().QuerySingleAsync<int>(sql, new
            {
                tableId,
                pkColumnName,
                pkValue
            });

            
            return count > 0;
        }

        
    }
}

