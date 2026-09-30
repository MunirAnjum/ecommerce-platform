using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;
using System.Net;
using System.Net.Sockets;

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

            var addresses = await Dns.GetHostAddressesAsync(_settings.SmtpServer);

            var ipv6Address = addresses.FirstOrDefault(
                address => address.AddressFamily == AddressFamily.InterNetworkV6);

            if (ipv6Address == null)
            {
                throw new InvalidOperationException(
                    $"No IPv6 address found for SMTP server '{_settings.SmtpServer}'.");
            }

            using var socket = new Socket(
                AddressFamily.InterNetworkV6,
                SocketType.Stream,
                ProtocolType.Tcp);

            await socket.ConnectAsync(ipv6Address, _settings.Port);

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                socket,
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