namespace PaymentService.Application.Events
{
    public class PaymentFailedEvent
    {
        public Guid PaymentId { get; set; }

        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public string Recipient { get; set; } = string.Empty;
    }
}
