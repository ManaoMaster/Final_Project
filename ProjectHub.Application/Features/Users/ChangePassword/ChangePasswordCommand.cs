using MediatR;

namespace ProjectHub.Application.Features.Users.ChangePassword
{
    
    
    public class ChangePasswordCommand : IRequest<Unit>
    {
        
        public int UserId { get; set; }

        
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}