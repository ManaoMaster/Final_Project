using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Tables.UpdateTable
{
    public class UpdateTableCommand : IRequest<TableResponseDto>
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public required string NewName { get; set; }
    }
}
