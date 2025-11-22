using System.Threading.Tasks;
using ProjectHub.Domain.Entities; 

namespace ProjectHub.Application.Interfaces 
{
    
    public interface IColumnRepository
    {
        Task<Columns> CreateColumnWithNewRelationshipAsync(Columns columnEntity, Relationships newRelationship);
        
        
        
        Task<bool> IsColumnNameUniqueForTableAsync(int tableId, string columnName);

        
        
        
        Task<bool> HasPrimaryKeyAsync(int tableId);

        
        
        
        Task AddColumnAsync(Columns column); 

        
        
        
        Task<Columns?> GetColumnByIdAsync(int columnId); 

        
        
        
        Task<IEnumerable<Columns>> GetColumnsByTableIdAsync(int tableId);

        Task UpdateColumnAsync(Columns column);

        Task<Columns?> GetColumnByNameAsync(int tableId, string name);
        Task DeleteColumnAsync(int columnId);
        Task<IEnumerable<Columns>> GetByIdsAsync(List<int> ids);

    }
}
