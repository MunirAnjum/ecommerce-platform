using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;

    public CartService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        return await MapToResponseAsync(cart);
    }

    public async Task<CartResponse> AddItemAsync(
        Guid userId,
        AddToCartRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        ValidateWeight(request.WeightInGrams);

        var product =
            await _productServiceClient.GetProductByIdAsync(
                request.ProductId);

        if (product is null || !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found or inactive.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem =
            await _cartRepository.GetItemAsync(
                userId,
                request.ProductId,
                request.WeightInGrams);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(
                existingItem.Quantity + request.Quantity);

            await _cartRepository.UpdateItemAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem(
                cart.Id,
                product.Id,
                request.Quantity,
                request.WeightInGrams);

            await _cartRepository.AddItemAsync(newItem);
        }

        return await MapToResponseAsync(cart);
    }

    public async Task<CartResponse> UpdateItemAsync(
        Guid userId,
        Guid productId,
        UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        ValidateWeight(request.WeightInGrams);

        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        var item =
            await _cartRepository.GetItemAsync(
                userId,
                productId,
                request.WeightInGrams);

        if (item is null)
        {
            throw new KeyNotFoundException(
                "Cart item not found.");
        }

        item.UpdateQuantity(request.Quantity);

        await _cartRepository.UpdateItemAsync(item);

        return await MapToResponseAsync(cart);
    }

    public async Task RemoveItemAsync(
    Guid userId,
    Guid productId,
    int weightInGrams)
    {
        var item = await _cartRepository.GetItemAsync(
            userId,
            productId,
            weightInGrams);

        if (item is null)
        {
            throw new KeyNotFoundException(
                "Cart item not found.");
        }

        await _cartRepository.RemoveItemAsync(item);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart(userId);

        await _cartRepository.AddAsync(cart);

        return cart;
    }

    private async Task<CartResponse> MapToResponseAsync(
        Cart cart)
    {
        var response = new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId
        };

        // Load cart items directly rather than relying on
        // a tracked Cart.Items collection.
        var items = await GetCartItemsAsync(cart.Id);

        foreach (var item in items)
        {
            var product =
                await _productServiceClient
                    .GetProductByIdAsync(item.ProductId);

            if (product is null)
            {
                continue;
            }

            var unitPrice = CalculateUnitPrice(product.Price, item.WeightInGrams);

            var totalPrice = unitPrice * item.Quantity;

            response.Items.Add(new CartItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                WeightInGrams = item.WeightInGrams,
                Quantity = item.Quantity,
                TotalPrice = totalPrice
            });

            response.TotalAmount += totalPrice;
        }

        return response;
    }

    private async Task<List<CartItem>> GetCartItemsAsync(
    Guid cartId)
    {
        return await _cartRepository.GetItemsAsync(cartId);
    }


    private static void ValidateWeight(int weightInGrams)
    {
        if (weightInGrams is not (250 or 500 or 1000))
        {
            throw new InvalidOperationException(
                "Weight must be 250g, 500g, or 1000g.");
        }
    }

    private static decimal CalculateUnitPrice(
        decimal basePrice,
        int weightInGrams)
    {
        return basePrice * weightInGrams / 250m;
    }
    
}