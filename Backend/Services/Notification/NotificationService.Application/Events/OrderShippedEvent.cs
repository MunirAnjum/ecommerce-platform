using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Events
{
    public class OrderShippedEvent
    {
        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public string Recipient { get; set; } = string.Empty;
    }
}
