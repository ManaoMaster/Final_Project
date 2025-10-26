using System;

namespace ProjectHub.Application.Dtos
{
    public class TableResponseDto
    {
        public int TableId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
