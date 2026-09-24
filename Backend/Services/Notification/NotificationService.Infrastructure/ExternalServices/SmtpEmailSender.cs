using MailKit.Security;
using MimeKit;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.ExternalServices
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string recipient, string subject, string message)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_settings.From));

            email.To.Add(MailboxAddress.Parse(recipient));

            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = message
            };

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.SmtpServer, 
                _settings.Port, 
                SecureSocketOptions.StartTls);

            await smtpClient.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await smtpClient.SendAsync(email);

            await smtpClient.DisconnectAsync(true);
        }
    }
}
