using NotificationService.Application.DTOs;
using NotificationService.Application.Events;
using NotificationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateAsync(CreateNotificationRequest request);

        Task<NotificationResponse> GetByIdAsync(Guid userId, Guid notificationId);

        Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId);

        Task<NotificationResponse> SendAsync(Guid userId, Guid notificationId);

        Task ProcessPaymentCompletedAsync(PaymentCompletedEvent paymentEvent);

        Task ProcessPaymentFailedAsync(PaymentFailedEvent paymentEvent);

        Task ProcessOrderCreatedAsync(OrderCreatedEvent orderEvent);

        Task ProcessOrderShippedAsync(OrderShippedEvent orderEvent);

        Task ProcessOrderDeliveredAsync(OrderDeliveredEvent orderEvent);
    }
}
