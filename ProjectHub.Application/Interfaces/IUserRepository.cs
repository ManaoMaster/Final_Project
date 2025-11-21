using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsEmailUniqueAsync(string email);
        Task AddUserAsync(Users user);
        Task<bool> ExistsAsync(int userId);

        Task<Users?> GetByIdAsync(int userId);
        Task<Users?> GetByEmailAsync(string email);

        Task<bool> IsEmailUsedByOtherAsync(int userId, string email);
        Task<bool> IsUsernameUsedByOtherAsync(int userId, string username);
        Task UpdateUserAsync(Users user);

        Task DeleteUserAsync(int userId);

        Task<Users?> GetUserByIdAsync(int userId);

        Task<List<Users>> GetAllAsync();
    }
}
