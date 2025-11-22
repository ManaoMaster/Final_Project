using MediatR;
using System.ComponentModel.DataAnnotations;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Users.EditProfile
{
   public class EditProfileCommand : IRequest<UserResponseDto>
   {
       public int UserId { get; set; } 
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string? ProfilePictureUrl { get; set; }
    }
}
