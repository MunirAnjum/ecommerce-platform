using PaymentService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces
{
    public interface IOrderServiceClient
    {
        Task<OrderDetailsResponse?> GetOrderByIdAsync(Guid orderId);

        Task ConfirmPaymentAsync(Guid orderId);
    }
}
