using PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; }

        public string? TransactionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        private Payment()
        {

        }

        public Payment(Guid orderId, Guid userId, decimal amount, PaymentMethod method)
        {
            if(orderId == Guid.Empty)
            {
                throw new ArgumentException("Order ID can not be empty.", nameof(orderId));
            }

            if(userId == Guid.Empty)
            {
                throw new ArgumentException("User ID can not be empty.", nameof(userId));
            }

            if(amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than zero.");
            }

            Id = Guid.NewGuid();
            OrderId = orderId;
            UserId = userId;
            Amount = amount;
            Method = method;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void StartProcessing()
        {
            if(Status != PaymentStatus.Pending && Status != PaymentStatus.Failed)
            {
                throw new InvalidOperationException("Only pending or failed payment can be processed.");
            }

            Status = PaymentStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete(string? transactionId = null)
        {
            if(Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("This payments can not be completed.");
            }

            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Fail()
        {
            if(Status != PaymentStatus.Processing)
            {
                throw new InvalidOperationException("Only processing payment can be fail.");
            }

            Status = PaymentStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if(Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Completed payment can not be cancelled.");
            }

            Status = PaymentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
