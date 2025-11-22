using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProjectHub.Application.Features.Projects.DeleteProject
{
    
    public class DeleteUserCommand : IRequest<Unit>
    {
        [Required]
        public int UserId { get; set; } 
    }
}
