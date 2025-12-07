using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

namespace GymManager.Api.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public SmtpEmailService(IConfiguration config) { _config = config; }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var port = int.Parse(smtp["Port"] ?? "587");
            var user = smtp["User"];
            var pass = smtp["Pass"];
            var from = smtp["From"];

            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(from));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}
