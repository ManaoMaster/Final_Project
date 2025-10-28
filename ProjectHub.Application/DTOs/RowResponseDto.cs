using System;

namespace ProjectHub.Application.Dtos
{
    public class RowResponseDto
    {
        public int RowId { get; set; }
        public string Data { get; set; } = string.Empty;
        public int TableId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
