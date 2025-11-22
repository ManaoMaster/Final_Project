using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    
    public class DeleteRowCommand : IRequest<Unit>
    {
        [Required]
        public int RowId { get; set; } 
    }
}
