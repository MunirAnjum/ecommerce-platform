using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);

    Task<Cart?> GetByIdAsync(Guid id);

    Task<CartItem?> GetItemAsync(Guid userId, Guid productId);

    Task<List<CartItem>> GetItemsAsync(Guid cartId);

    Task AddAsync(Cart cart);

    Task AddItemAsync(CartItem item);

    Task UpdateItemAsync(CartItem item);

    Task RemoveItemAsync(CartItem item);

    Task ClearItemsAsync(Guid cartId);

}