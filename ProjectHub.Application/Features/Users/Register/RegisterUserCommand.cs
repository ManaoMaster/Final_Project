using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Users.Register
{
    // 2. เปลี่ยน return type จาก int เป็น UserResponseDto
    public class RegisterUserCommand : IRequest<UserResponseDto>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
