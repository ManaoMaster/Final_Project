using MediatR;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Features.Users.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IPasswordResetTokenRepository _tokens;
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher; // หรือ service ที่ใช้อยู่แล้ว

        public ResetPasswordHandler(
            IPasswordResetTokenRepository tokens,
            IUserRepository users,
            IPasswordHasher hasher)
        {
            _tokens = tokens;
            _users = users;
            _hasher = hasher;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
        {
            var token = await _tokens.GetValidTokenAsync(request.Token);
            if (token is null || token.Used || token.ExpiresAt < DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid or expired token.");
            }

            var user = await _users.GetByIdAsync(token.UserId);
            if (user is null)
                throw new ArgumentException("User not found.");

            user.Password = _hasher.Hash(request.NewPassword);
            token.Used = true;

            await _tokens.SaveChangesAsync();
        }
    }
}
