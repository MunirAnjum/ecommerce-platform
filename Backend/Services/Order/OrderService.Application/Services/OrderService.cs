using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
    }

    public async Task<OrderResponse> CreateAsync(   
        Guid userId,
        CreateOrderRequest request)
    {
        var cart = await _cartRepository.GetByIdAsync(
            request.CartId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        if (cart.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot access this cart.");
        }

        var cartItems = await _cartRepository.GetItemsAsync(
            cart.Id);

        if (!cartItems.Any())
        {
            throw new InvalidOperationException(
                "Cannot create order from an empty cart.");
        }

        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cartItems)
        {
            var product =
                await _productServiceClient
                    .GetProductByIdAsync(cartItem.ProductId);

            if (product is null || !product.IsActive)
            {
                throw new KeyNotFoundException(
                    $"Product {cartItem.ProductId} was not found or is inactive.");
            }

            var orderItem = new OrderItem(
                product.Id,
                product.Name,
                product.Price,
                cartItem.Quantity);
                
            orderItems.Add(orderItem);
        }

        foreach(var item in orderItems)
        {
            await _inventoryServiceClient.ReserveStockAsync(item.ProductId, item.Quantity);
        }

        var totalAmount = orderItems.Sum(x => x.TotalPrice);

        var order = new Order(
            userId,
            totalAmount);

        foreach (var item in orderItems)
        {
            item.SetOrderId(order.Id);

            order.Items.Add(item);
        }

        await _orderRepository.AddAsync(order);

        await _cartRepository.ClearItemsAsync(cart.Id);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> GetByIdAsync(
        Guid userId,
        Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(
            orderId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot access this order.");
        }

        return MapToResponse(order);
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(
        Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<OrderResponse>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<OrderResponse> GetByIdForAdminAsync(
    Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CancelAsync(
    Guid userId,
    Guid orderId,
    bool isAdmin)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        // Customer can cancel only their own order.
        if (!isAdmin && order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot access this order.");
        }

        if (order.Status == OrderStatus.Pending)
        {
            // Inventory is still reserved.
            foreach (var item in order.Items)
            {
                await _inventoryServiceClient.ReleaseStockAsync(
                    item.ProductId,
                    item.Quantity);
            }
        }
        else if (order.Status == OrderStatus.Confirmed)
        {
            // Inventory was already confirmed/consumed.
            // Do NOT release it here.
        }
        else
        {
            throw new InvalidOperationException(
                "Only pending or confirmed orders can be cancelled.");
        }

        order.Cancel();

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if(order is null)
        {
            throw new Exception("Order not found.");
        }

        switch(request.Status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;

            case OrderStatus.Processing:
                order.Process(); 
                break;

            case OrderStatus.Shipped:
                order.Ship();
                break;

            case OrderStatus.Delivered:
                order.Deliver(); 
                break;

            case OrderStatus.Cancelled:

                if (order.Status == OrderStatus.Pending)
                {
                    foreach (var item in order.Items)
                    {
                        await _inventoryServiceClient.ReleaseStockAsync(
                            item.ProductId,
                            item.Quantity);
                    }
                }
                else if (order.Status != OrderStatus.Confirmed)
                {
                    throw new InvalidOperationException(
                        "Only pending or confirmed orders can be cancelled.");
                }

                order.Cancel();
                break;

            default:
                throw new InvalidOperationException("Invalid order status.");
        }

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> ConfirmPaymentAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if(order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        if(order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending order can be confirm after payment.");
        }

        foreach(var item in order.Items)
        {
            await _inventoryServiceClient.ConfirmStockAsync(item.ProductId, item.Quantity);
        }

        order.Confirm();

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(
        Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,

            Items = order.Items
                .Select(x => new OrderItemResponse
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    TotalPrice = x.TotalPrice
                })
                .ToList()
        };
    }
}