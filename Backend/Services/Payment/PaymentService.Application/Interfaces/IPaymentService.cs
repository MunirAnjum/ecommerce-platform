using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreateAsync(Guid userId, CreatePaymentRequest request);

        Task<PaymentResponse> ProcessAsync(Guid userId, Guid paymentId, ProcessPaymentRequest request);

        Task<PaymentResponse> GetByIdAsync(Guid userId, Guid paymentId);

        Task<List<PaymentResponse>> GetMyPaymentsAsync(Guid userId);

        Task<PaymentResponse> ConfirmCashOnDeliveryAsync(Guid userId, Guid paymentId);
    }
}
    