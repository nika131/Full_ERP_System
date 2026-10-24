using Microsoft.Extensions.Configuration;
using NexusERP.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace NexusERP.Infrastructure.Services
{
    public class SmtpVerificationEmailSender : IVerificationEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpVerificationEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationCodeAsync(string email, string code)
        {
            string host = _configuration["Email:Host"] ?? throw new InvalidOperationException("SMTP host is missing.");
            string from = _configuration["Email:From"] ?? throw new InvalidOperationException("SMTP sender address is missing.");
            string username = _configuration["Email:Username"] ?? throw new InvalidOperationException("SMTP username is missing.");
            string password = _configuration["Email:Password"] ?? throw new InvalidOperationException("SMTP password is missing.");

            int port = int.TryParse(_configuration["Email:Port"], out int configuredPort) ? configuredPort : 587;

            using var message = new MailMessage
            {
                From = new MailAddress(from, "TenexERP"),
                Subject = "Verify your TenexERP email",
                Body = $"Your TenexERP verification code is: {code}\n\nThis code expires in 10 minutes.\n\nIf you did not request this code, you can ignore this email.",
                IsBodyHtml = false
            };

            message.To.Add(email);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 15000
            };

            await client.SendMailAsync(message);
        }
    }
}