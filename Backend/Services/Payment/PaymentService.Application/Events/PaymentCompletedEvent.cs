using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Events
{
    public class PaymentCompletedEvent
    {
        public Guid PaymentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public string Recipient { get; set; } = string.Empty;
    }
}
