using MediatR;

namespace ProjectHub.Application.Features.Users.ResetPassword
{
    public class ResetPasswordCommand : IRequest
    {
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
