using MediatR;
using ProjectHub.Application.Dtos; 
using System.ComponentModel.DataAnnotations;


namespace ProjectHub.Application.Features.Rows.UpdateRow 
{
    
    public class UpdateRowCommand : IRequest<RowResponseDto>
    {
        [Required]
        public int RowId { get; set; } 

        [Required]
        [MaxLength(100)]
        public string NewData { get; set; } = string.Empty; 
    }
}
