using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories
{
    
    public class ProjectRepository : IProjectRepository
    {
        
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task AddProjectAsync(Projects project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        
        
        public async Task<bool> IsProjectNameUniqueForUserAsync(int userId, string projectName)
        {
            
            
            return await _context.Projects.AnyAsync(p =>
                p.User_id == userId && p.Name == projectName
            );
        }

        public async Task<Projects?> GetProjectByIdAsync(int projectId)
        {
            
            
            return await _context.Projects
            .Include(p => p.Tables) 
            .FirstOrDefaultAsync(p => p.Project_id == projectId);
        }

        public async Task UpdateProjectAsync(Projects project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            
            var projectToDelete = await _context.Projects.FindAsync(projectId);

            
            if (projectToDelete != null)
            {
                _context.Projects.Remove(projectToDelete);
                await _context.SaveChangesAsync();
                
            }
            
        }

        public async Task<IEnumerable<Projects>> GetProjectsByUserIdAsync(int userId)
        {
            
            
            
            return await _context.Projects
                   .Where(p => p.User_id == userId)
                           .Include(p => p.Tables) 
            .AsNoTracking()
                   .ToListAsync();
        }
        public async Task UpdateTimestampsAsync(Projects project)
        {
            
            
            _context.Projects.Attach(project);

            
            _context.Entry(project).Property(p => p.LastOpenedAt).IsModified = true;

            

            
            await _context.SaveChangesAsync();
        }

    }
}
