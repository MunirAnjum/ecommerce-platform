using OrderService.Application.DTOs;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces;
public interface IOrderService
{
    Task<OrderResponse> CreateAsync(Guid userId, CreateOrderRequest request);

    Task<OrderResponse> GetByIdAsync(Guid userId, Guid orderId);

    Task<List<OrderResponse>> GetMyOrdersAsync(Guid userId);

    Task<OrderResponse> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request);

    Task<List<OrderResponse>> GetAllAsync();

    Task<OrderResponse> GetByIdForAdminAsync(Guid orderId);

    Task<OrderResponse> CancelAsync(Guid userId, Guid orderId, bool isAdmin);

    Task<OrderResponse> ConfirmPaymentAsync(Guid orderId);
}
