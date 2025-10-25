using System;

namespace ProjectHub.Application.DTOs
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
