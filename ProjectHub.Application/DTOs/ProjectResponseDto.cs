using System;

namespace ProjectHub.Application.Dtos
{
    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        
        

        
        public DateTime UpdatedAt { get; set; }

        
        public int TableCount { get; set; }

        
        public bool IsFavorite { get; set; }

        
        public DateTime LastOpenedAt { get; set; }
    }
}