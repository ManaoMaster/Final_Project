using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.Dtos;

namespace ProjectHub.Application.Features.Tables.DeleteTable
{
    public class DeleteTableCommand : IRequest<Unit>
    {
        [Required]
        public int TableId { get; set; }
    }
}
