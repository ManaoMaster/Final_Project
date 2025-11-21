using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task AddAsync(PasswordResetToken token);
        Task SaveChangesAsync();
    }
}
