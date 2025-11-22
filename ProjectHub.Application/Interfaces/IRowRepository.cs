using System.Threading.Tasks;
using ProjectHub.Domain.Entities; 

namespace ProjectHub.Application.Repositories
{
    
    public interface IRowRepository
    {
        
        
        
        Task AddRowAsync(Rows row);
        Task<Rows?> GetRowByIdAsync(int rowId);

        Task UpdateRowAsync(Rows row);
        

        Task DeleteRowAsync(int rowId);

        Task<int> GetMaxPkValueAsync(int tableId, string pkColumnName);

        Task<bool> IsPkValueDuplicateAsync(int tableId, string pkColumnName, string pkValue);
    }
}
