using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Repositories // ต้องใช้ Namespace นี้
{
    public interface ITableRepository
    {
        Task<bool> IsTableNameUniqueForProjectAsync(int ProjectId, string tableName);
        Task AddTableAsync(Tables table);
    }
}
