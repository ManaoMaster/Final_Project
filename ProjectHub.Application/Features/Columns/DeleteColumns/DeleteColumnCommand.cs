using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Columns.DeleteColumn
{
    
    public class DeleteColumnCommand : IRequest<Unit>
    {
        [Required]
        public int ColumnId { get; set; } 
    }
}
