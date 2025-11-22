using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces 
{
    public interface IProjectRepository
    {
        Task<bool> IsProjectNameUniqueForUserAsync(int userId, string projectName);
        Task AddProjectAsync(Projects project);

        Task<Projects?> GetProjectByIdAsync(int projectId); 

        Task<IEnumerable<Projects>> GetProjectsByUserIdAsync(int userId);

        Task UpdateProjectAsync(Projects project);

        Task DeleteProjectAsync(int projectId);
        Task UpdateTimestampsAsync(Projects project);
    }
}
