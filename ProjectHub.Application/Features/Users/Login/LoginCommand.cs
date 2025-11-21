using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Users.Login
{
    public class LoginCommand : IRequest<TokenResponseDto>
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        
    }
}
