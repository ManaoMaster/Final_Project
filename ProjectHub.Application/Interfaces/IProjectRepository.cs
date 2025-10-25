using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Repositories // ต้องใช้ Namespace นี้
{
    public interface IProjectRepository
    {
        Task<bool> IsProjectNameUniqueForUserAsync(int userId, string projectName);
        Task AddProjectAsync(Projects project);
    }
}
