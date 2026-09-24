using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Messaging
{
    public class RabbitMqConstants
    {
        public const string EventExchange = "ecommerce.events";

        public const string PaymentCompletedRoutingKey = "payment.completed";

        public const string PaymentFailedRoutingKey = "payment.failed";
    }
}
