using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Users.Login
{
    public interface IJwtTokenService
    {
        TokenResponseDto GenerateForUser(int userId, string email, string username, string role);
    }
}
