using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsEmailUniqueAsync(string email);
        Task AddUserAsync(Users user);
    }
}