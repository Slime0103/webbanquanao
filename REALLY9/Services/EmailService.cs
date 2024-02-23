using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using REALLY9.ModelViews;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace REALLY9.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Name", _smtpSettings.Username));
            mimeMessage.To.Add(new MailboxAddress("", email));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

                // Xác thực nếu cần
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                // Gửi email
                await client.SendAsync(mimeMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}