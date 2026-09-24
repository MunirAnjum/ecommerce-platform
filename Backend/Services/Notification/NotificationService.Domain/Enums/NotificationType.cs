using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Domain.Enums
{
    public enum NotificationType
    {
        OrderCreated = 1,
        PaymentCompleted = 2,
        PaymentFailed = 3,
        OrderShipped = 4,
        OrderDelivered = 5,
        PasswordReset = 6
    }
}
