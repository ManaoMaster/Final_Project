using System.ComponentModel.DataAnnotations; // ต้องใช้ [Required]
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Rows.CreateRow
{
    public class CreateRowCommand : IRequest<RowResponseDto>
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public string Data { get; set; } = string.Empty;
    }
}
