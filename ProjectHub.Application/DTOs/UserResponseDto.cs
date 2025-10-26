using System;

namespace ProjectHub.Application.Dtos
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
