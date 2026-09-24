using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Events
{
    public class OrderDeliveredEvent
    {
        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public string Recipient { get; set; } = string.Empty;
    }
}
