using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Infrastructure.Persistence; 


namespace ProjectHub.Infrastructure.Repositories
{
    
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task AddUserAsync(Users user)
        {
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            
            
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        
        public Task<bool> ExistsAsync(int userId) =>
            _context.Users.AnyAsync(u => u.User_id == userId);

        
        public Task<Users?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<Users?> GetByIdAsync(int userId) =>
            _context.Users.FirstOrDefaultAsync(u => u.User_id == userId);

        public Task<bool> IsUsernameUsedByOtherAsync(int userId, string username) =>
            _context.Users.AnyAsync(u => u.User_id != userId && u.Username == username);

        public Task<bool> IsEmailUsedByOtherAsync(int userId, string email) =>
            _context.Users.AnyAsync(u =>
                u.User_id != userId && u.Email.ToLower() == email.ToLower()
            );

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            
            return await _context.Users.FindAsync(userId);
        }

        public async Task DeleteUserAsync(int userId)
        {
            
            var userToDelete = await _context.Users.FindAsync(userId);

            
            if (userToDelete != null)
            {
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();
                
            }
            
        }

        public async Task<List<Users>> GetAllAsync()
        {
            
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateAsync(Users user)
        {
            _context.Users.Update(user);          
            await _context.SaveChangesAsync();    
        }
    }
}
