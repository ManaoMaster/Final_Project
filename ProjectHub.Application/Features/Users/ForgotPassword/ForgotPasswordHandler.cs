using MediatR;
using Microsoft.Extensions.Configuration;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Features.Users.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetTokenRepository _tokens;
        private readonly IEmailSender _email;
        private readonly IConfiguration _config;

        public ForgotPasswordHandler(
            IUserRepository users,
            IPasswordResetTokenRepository tokens,
            IEmailSender email,
            IConfiguration config)
        {
            _users = users;
            _tokens = tokens;
            _email = email;
            _config = config;
        }

        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            
            var user = await _users.GetByEmailAsync(request.Email);
            if (user is null)
                throw new ArgumentException("Email not found.");

            
            var tokenValue = Guid.NewGuid().ToString("N");

            var token = new PasswordResetToken
            {
                UserId = user.User_id,           
                Token = tokenValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Used = false,
            };

            await _tokens.AddAsync(token);
            await _tokens.SaveChangesAsync();

            
            var frontendBase = _config["Frontend:BaseUrl"]?.TrimEnd('/')
                               ?? "http://localhost:4200";

            var resetLink = $"{frontendBase}/reset-password?token={tokenValue}";

            var subject = "ProjectHub – Reset your password";
            var body = $@"
สวัสดี {user.Username},

มีการขอ reset password สำหรับบัญชี ProjectHub ของคุณ
ถ้าเป็นคุณเอง ให้คลิกลิงก์ด้านล่างนี้ภายใน 30 นาที:

{resetLink}

ถ้าไม่ใช่คุณ กรุณาเพิกเฉยอีเมลฉบับนี้
";

            await _email.SendAsync(user.Email, subject, body);
        }
    }
}
