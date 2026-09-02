using OrderService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces;
public interface ICartService
{
    Task<CartResponse> GetCartAsync(Guid userId);

    Task<CartResponse> AddItemAsync(Guid userId, AddToCartRequest request);

    Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request);

    Task RemoveItemAsync(Guid userId, Guid productId);
}
