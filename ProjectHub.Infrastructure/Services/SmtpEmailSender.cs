using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var section = _config.GetSection("Smtp");

            var host = section["Host"]!;
            var port = int.Parse(section["Port"]!);
            var useSsl = bool.Parse(section["UseSsl"]!);
            var user = section["User"]!;
            var password = section["Password"]!;
            var fromEmail = section["FromEmail"] ?? user;
            var fromName = section["FromName"] ?? "ProjectHub";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                UseDefaultCredentials = false,                 // ✅ สำคัญ
                Credentials = new NetworkCredential(user, password),
            };

            var msg = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };

            msg.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(msg);
        }
    }
}
