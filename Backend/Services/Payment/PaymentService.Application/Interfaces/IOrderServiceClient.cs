using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces
{
    public interface IOrderServiceClient
    {
        Task<OrderDetailsResponse?> GetOrderByIdAsync(Guid orderId);

        Task ConfirmPaymentAsync(Guid orderId);

        Task AcceptCashOnDeliveryAsync(Guid orderId);
    }
}
