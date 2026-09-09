using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _createValidator;

    public OrderController(
        IOrderService orderService,
        IValidator<CreateOrderRequest> createValidator)
    {
        _orderService = orderService;
        _createValidator = createValidator;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        CreateOrderRequest request)
    {
        var validationResult =
            await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = GetUserId();

        var order =
            await _orderService.CreateAsync(userId, request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyOrder()
    {
        var userId = GetUserId();

        var orders =
            await _orderService.GetMyOrdersAsync(userId);

        return Ok(orders);
    }

    [HttpPut("{orderId:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateStatusAsync(orderId, request);

        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllAsync();

        return Ok(orders);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrderForAdmin(Guid id)
    {
        var order = await _orderService.GetByIdForAdminAsync(id);

        return Ok(order);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();

        var order =
            await _orderService.GetByIdAsync(userId, id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [Authorize(Policy = "InternalService")]
    [HttpGet("internal/{id:guid}")]
    public async Task<IActionResult> GetByIdInternal(Guid id)
    {
        var order = await _orderService.GetByIdForAdminAsync(id);

        return Ok(order);
    }

    [Authorize(Policy = "InternalService")]
    [HttpPost("internal/{id:guid}/payment-confirmed")]
    public async Task<IActionResult> PaymentConfirmed(Guid id)
    {
        var order = await _orderService.ConfirmPaymentAsync(id);

        return Ok(order);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAsync(Guid id)
    {
        var userId = GetUserId();

        var isAdmin = User.IsInRole("Admin");

        var order = await _orderService.CancelAsync(userId, id, isAdmin);

        return Ok(order);
    }

    private Guid GetUserId()
    {
        var value =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity");
        }

        return userId;
    }
}