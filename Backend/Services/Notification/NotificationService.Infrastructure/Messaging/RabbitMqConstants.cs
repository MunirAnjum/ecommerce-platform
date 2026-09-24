using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Messaging
{
    public class RabbitMqConstants
    {
        public const string EventExchange = "ecommerce.events";

        public const string PaymentCompletedRoutingKey = "payment.completed";

        public const string PaymentFailedRoutingKey = "payment.failed";

        public const string OrderCreatedRoutingKey = "order.created";

        public const string OrderShippedRoutingKey = "order.shipped";

        public const string OrderDeliveredRoutingKey = "order.delivered";
    }
}
