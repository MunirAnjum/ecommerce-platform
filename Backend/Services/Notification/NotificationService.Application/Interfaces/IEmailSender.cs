using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string recipient, string subject, string message);
    }
}
