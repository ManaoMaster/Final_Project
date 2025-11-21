using MediatR;

namespace ProjectHub.Application.Features.Users.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest
    {
        public string Email { get; set; } = default!;
    }
}
