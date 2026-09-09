using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
	{
        _cartService = cartService;
	}

    [HttpGet]
    public async Task<IActionResult> GetCarts()
    {
        var userId = GetUserId();

        var cart = await _cartService.GetCartAsync(userId);

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartRequest request)
    {
        var userId = GetUserId();

        var cart = await _cartService.AddItemAsync(userId, request);

        return Ok(cart);
    }

    [HttpPut("item/{productId:guid}")]
    public async Task<IActionResult> UpdateItemAsync(Guid productId, UpdateCartItemRequest request)
    {
        var userId = GetUserId();

        var cart = await _cartService.UpdateItemAsync(userId, productId, request);

        return Ok(cart);
    }

    [HttpDelete("item/{productId:guid}")]
    public async Task<IActionResult> RemoveItemAsync(Guid productId)
    {
        var userId = GetUserId();

        await _cartService.RemoveItemAsync(userId, productId);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity.");
        }

        return userId;
    }
}