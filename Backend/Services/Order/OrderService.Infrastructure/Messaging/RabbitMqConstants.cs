using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging
{
    public static class RabbitMqConstants
    {
        public const string EventExchange = "ecommerce.events";

        public const string OrderCreatedRoutingKey = "order.created";

        public const string OrderShippedRoutingKey = "order.shipped";

        public const string OrderDeliveredRoutingKey = "order.delivered";
    }
}
