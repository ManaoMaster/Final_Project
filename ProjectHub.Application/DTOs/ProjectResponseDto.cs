using System;

namespace ProjectHub.Application.Dtos
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
