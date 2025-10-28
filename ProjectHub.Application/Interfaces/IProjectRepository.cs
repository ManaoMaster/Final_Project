using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces // ต้องใช้ Namespace นี้
{
    public interface IProjectRepository
    {
        Task<bool> IsProjectNameUniqueForUserAsync(int userId, string projectName);
        Task AddProjectAsync(Projects project);

        Task<Projects?> GetProjectByIdAsync(int projectId); // คืนค่า Project หรือ null ถ้าไม่เจอ

        Task UpdateProjectAsync(Projects project);
    }
}
