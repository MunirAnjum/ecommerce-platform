using NotificationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Recipient { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsSent { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }

        public Guid? SourceId { get; set; }

        public Notification(
            Guid userId,
            string recipient,
            string subject,
            string message,
            NotificationType type)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(
                    "User ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(recipient))
                throw new ArgumentException(
                    "Recipient is required.");

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException(
                    "Subject is required.");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(
                    "Message is required.");

            Id = Guid.NewGuid();
            UserId = userId;
            Recipient = recipient;
            Subject = subject;
            Message = message;
            Type = type;
            IsSent = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsSent()
        {
            IsSent = true;
            SentAt = DateTime.UtcNow;
        }
    }
}
