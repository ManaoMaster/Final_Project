using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces 
{
    public interface ITableRepository
    {
        Task<bool> IsTableNameUniqueForProjectAsync(int ProjectId, string tableName);
        Task AddTableAsync(Tables table);
        Task<IEnumerable<Tables>> GetTablesByProjectIdAsync(int projectId);
        Task<Tables?> GetTableByIdAsync(int tableId);
        Task UpdateTableAsync(Tables tableToUpdate);

        Task DeleteTableAsync(int tableId);
    }
}
