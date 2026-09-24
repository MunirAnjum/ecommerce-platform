using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string RoutingKey { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime OccurredOnUtc { get; set; }

        public DateTime? ProcessedOnUtc { get; set; }

        public string? Error { get; set; }
    }
}
