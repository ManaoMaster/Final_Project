using MediatR;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Repositories;
using BCrypt.Net;

namespace ProjectHub.Application.Features.Users.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, TokenResponseDto>
    {
        private readonly IUserRepository _users;
        private readonly IJwtTokenService _jwt;

        public LoginHandler(IUserRepository users, IJwtTokenService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        public async Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await _users.GetByEmailAsync(request.Email)
                       ?? throw new ArgumentException("Invalid email or password.");

            var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!ok) throw new ArgumentException("Invalid email or password.");

            return _jwt.GenerateForUser(user.User_id, user.Email, user.Username);
        }
    }
}
