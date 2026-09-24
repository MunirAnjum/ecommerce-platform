using NotificationService.Application.DTOs;
using NotificationService.Application.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailSender _emailSender;

        public NotificationService(INotificationRepository notificationRepository, IEmailSender emailSender)
        {
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
        }


        public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request)
        {
            var notification = new Notification(
                request.UserId,
                request.Recipient,
                request.Subject,
                request.Message,
                request.Type
                );

            await _notificationRepository.AddAsync(notification);

            return MapToResponse(notification);
        }

        public async Task<NotificationResponse> GetByIdAsync(Guid userId, Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if(notification is null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }

            if(notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can not access this notification.");
            }

            return MapToResponse(notification);
        }

        public async Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            return notifications.Select(MapToResponse).ToList();
        }

        public async Task<NotificationResponse> SendAsync(Guid userId, Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if(notification is null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }

            if(notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You cannot send this notification.");
            }

            if (notification.IsSent)
            {
                throw new InvalidOperationException("Notification has already been sent.");
            }


            await _emailSender.SendAsync(notification.Recipient, notification.Subject, notification.Message);

            notification.MarkAsSent();

            await _notificationRepository.UpdateAsync(notification);

            return MapToResponse(notification);

        }

        public async Task ProcessPaymentCompletedAsync(PaymentCompletedEvent paymentEvent)
        {
            var existingNotification = await _notificationRepository.GetBySourceIdAsync(
                paymentEvent.PaymentId,
                NotificationType.PaymentCompleted);

            if(existingNotification is not null)
            {
                return;
            }

            var notification = new Notification(
                paymentEvent.UserId,
                paymentEvent.Recipient,
                "Payment Completed",
                $"Your payment for order {paymentEvent.OrderId} " +
                $"has been completed successfully. " +
                $"Amount {paymentEvent.Amount}.",
                NotificationType.PaymentCompleted);

            notification.SourceId = paymentEvent.PaymentId;

            await _notificationRepository.AddAsync(notification);

            await _emailSender.SendAsync(notification.Recipient, notification.Subject, notification.Message);

            notification.MarkAsSent();

            await _notificationRepository.UpdateAsync(notification);
        }
        
        public async Task ProcessPaymentFailedAsync(PaymentFailedEvent paymentEvent)
        {
            var existingNotification = await _notificationRepository.GetBySourceIdAsync(
                paymentEvent.PaymentId,
                NotificationType.PaymentFailed);

            if(existingNotification is not null)
            {
                return;
            }

            var notification = new Notification(
                paymentEvent.UserId,
                paymentEvent.Recipient,
                "Payment Failed",
                $"Your payment for order {paymentEvent.OrderId} " +
                $"could not be completed. " +
                $"Amount {paymentEvent.Amount}",
                NotificationType.PaymentFailed);

            notification.SourceId = paymentEvent.PaymentId;

            await _notificationRepository.AddAsync(notification);

            await _emailSender.SendAsync(
                notification.Recipient,
                notification.Subject,
                notification.Message);

            notification.MarkAsSent();

            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task ProcessOrderCreatedAsync(OrderCreatedEvent orderEvent)
        {
            var existingNotification = await _notificationRepository.GetBySourceIdAsync(
                orderEvent.OrderId,
                NotificationType.OrderCreated);

            if(existingNotification is not null )
            {
                return;
            }

            var notification = new Notification(
                orderEvent.UserId,
                orderEvent.Recipient,
                "Order Created",
                $"Your order {orderEvent.OrderId} " +
                $"has been created successfully. " +
                $"Total amount: {orderEvent.TotalAmount}",
                NotificationType.OrderCreated);

            notification.SourceId = orderEvent.OrderId;

            await _notificationRepository.AddAsync(notification);

            await _emailSender.SendAsync(notification.Recipient, notification.Subject, notification.Message);

            notification.MarkAsSent();

            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task ProcessOrderShippedAsync(OrderShippedEvent orderEvent)
        {
            var existingNotification = await _notificationRepository.GetBySourceIdAsync(
                orderEvent.OrderId,
                NotificationType.OrderShipped);

            if(existingNotification is not null)
            {
                return;
            }

            var notification = new Notification(
                orderEvent.UserId,
                orderEvent.Recipient,
                "Order Shipped",
                $"Your order {orderEvent.OrderId} has been shipped.",
                NotificationType.OrderShipped);

            notification.SourceId = orderEvent.OrderId;

            await _notificationRepository.AddAsync(notification);

            await _emailSender.SendAsync(notification.Recipient, notification.Subject, notification.Message);

            notification.MarkAsSent();

            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task ProcessOrderDeliveredAsync(OrderDeliveredEvent orderEvent)
        {
            var existingNotification = await _notificationRepository.GetBySourceIdAsync(
                orderEvent.OrderId,
                NotificationType.OrderDelivered);

            if(existingNotification is not null)
            {
                return;
            }

            var notification = new Notification(
                orderEvent.UserId,
                orderEvent.Recipient,
                "Order Delivered",
                $"Your order {orderEvent.OrderId} has been delivered. ",
                NotificationType.OrderDelivered);

            notification.SourceId = orderEvent.OrderId;

            await _notificationRepository.AddAsync(notification);

            await _emailSender.SendAsync(notification.Recipient, notification.Subject, notification.Message);

            notification.MarkAsSent();
            
            await _notificationRepository.UpdateAsync(notification);
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Recipient = notification.Recipient,
                Subject = notification.Subject,
                Message = notification.Message,
                Type = notification.Type,
                IsSent = notification.IsSent,
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt
            };
        }
    }
}
