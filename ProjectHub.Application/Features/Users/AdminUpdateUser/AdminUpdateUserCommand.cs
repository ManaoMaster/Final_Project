using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Users.AdminUpdateUser
{
    public class AdminUpdateUserCommand : IRequest<UserResponseDto>
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public string? Role { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
