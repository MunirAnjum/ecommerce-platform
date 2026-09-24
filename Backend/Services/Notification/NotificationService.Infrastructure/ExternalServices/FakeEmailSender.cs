using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.ExternalServices
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendAsync(string recipient, string subject, string message)
        {
            Console.WriteLine("========== FAKE EMAIL ==========");
            Console.WriteLine($"To: {recipient}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine("================================");

            return Task.CompletedTask;
        }
    }
}
