using System.ComponentModel.DataAnnotations;
using MediatR;
using ProjectHub.Application.DTOs;


namespace ProjectHub.Application.Features.Tables.CreateTable
{
    public class CreateTableCommand : IRequest<TableResponseDto>
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
