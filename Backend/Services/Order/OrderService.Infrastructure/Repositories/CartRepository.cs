using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistences;

namespace OrderService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly OrderDbContext _context;

    public CartRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<Cart?> GetByIdAsync(Guid id)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CartItem?> GetItemAsync(
        Guid userId,
        Guid productId)
    {
        return await _context.CartItems
            .Where(x => x.ProductId == productId)
            .Where(x => x.Cart.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CartItem>> GetItemsAsync(Guid cartId)
    {
        return await _context.CartItems
            .Where(x => x.CartId == cartId)
            .ToListAsync();
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
    }

    public async Task AddItemAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        await _context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task ClearItemsAsync(Guid cartId)
    {
        var items = await _context.CartItems.Where(x => x.CartId == cartId).ToListAsync();

        if (items.Count == 0)
        {
            return;
        }

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}