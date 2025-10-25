using System;

namespace ProjectHub.Application.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
