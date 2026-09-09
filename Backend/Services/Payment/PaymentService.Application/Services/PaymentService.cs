using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderServiceClient _orderServiceClient;

        public PaymentService(IPaymentRepository paymentRepository, IOrderServiceClient orderServiceClient)
        {
            _paymentRepository = paymentRepository;
            _orderServiceClient = orderServiceClient;
        }

        public async Task<PaymentResponse> CreateAsync(Guid userId, CreatePaymentRequest request)
        {
            var order = await _orderServiceClient.GetOrderByIdAsync(request.OrderId);

            if(order is null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if(order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can not create payment for this order.");
            }

            if(order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Payment can only be created for a pending order");
            }

            var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);

            if(existingPayment is not null)
            {
                throw new InvalidOperationException("A payment already exist for this order.");
            }

            var payment = new Payment(
                order.Id,
                order.UserId,
                order.TotalAmount,
                request.Method
                );

            await _paymentRepository.AddAsync(payment);

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> ProcessAsync(Guid userId, Guid paymentId, ProcessPaymentRequest request)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            if (payment.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot process this payment.");
            }

            if(payment.Method == Domain.Enums.PaymentMethod.CashOnDelivery)
            {
                throw new InvalidOperationException("Cash on Delivery does not require online processing.");
            }

            payment.StartProcessing();

            if (request.ShouldSucceed)
            {
                payment.Complete($"FAKE-{Guid.NewGuid()}");
            }
            else
            {
                payment.Fail();
            }

            await _paymentRepository.UpdateAsync(payment);

            if (payment.Status == PaymentStatus.Completed)
            {
                await _orderServiceClient.ConfirmPaymentAsync(payment.OrderId);
            }

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> GetByIdAsync(Guid userId, Guid paymentId)
        {
            var payment =
                await _paymentRepository.GetByIdAsync(paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            if (payment.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot access this payment.");
            }

            return MapToResponse(payment);
        }

        public async Task<List<PaymentResponse>> GetMyPaymentsAsync(Guid userId)
        {
            var payments =
                await _paymentRepository.GetByUserIdAsync(userId);

            return payments
                .Select(MapToResponse)
                .ToList();
        }
        
        public async Task<PaymentResponse> ConfirmCashOnDeliveryAsync(Guid userId, Guid paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if(payment is null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if(payment.UserId != userId)
            {
                throw new InvalidOperationException("You cannot confirm this payment.");
            }

            if(payment.Method != PaymentMethod.CashOnDelivery)
            {
                throw new InvalidOperationException("Only Cash on Delivery payments can be confirmed this way.");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("Only pending Cash on Delivery payments can be confirmed.");
            }

            payment.Complete();

            await _paymentRepository.UpdateAsync(payment);

            await _orderServiceClient.ConfirmPaymentAsync(payment.OrderId);

            return MapToResponse(payment);
        }

        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }

}
