using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryRequest request)
    {
        var result = await _inventoryService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetByProductId),
            new { productId = request.ProductId },
            result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _inventoryService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var result = await _inventoryService.GetByProductIdAsync(productId);

        return Ok(result);
    }

    [HttpPost("{productId:guid}/stock")]
    public async Task<IActionResult> AddStock(Guid productId, UpdateStockQuantity request)
    {
        var result = await _inventoryService.AddStockAsync(productId, request);

        return Ok(result);
    }

    [HttpPost("{productId:guid}/reserve")]
    public async Task<IActionResult> ReserveStock(Guid productId, ReserveStockRequest request)
    {
        var result = await _inventoryService.ReserveStockAsync(productId, request);

        return Ok(result);
    }

    [HttpPost("{productId:guid}/release")]
    public async Task<IActionResult> ReleaseStock(Guid productId, ReserveStockRequest reserve)
    {
        var result = await _inventoryService.ReleaseStockAsync(productId, reserve);

        return Ok(result);
    }

    [HttpPost("{productId:guid}/confirm")]
    public async Task<IActionResult> ConfirmReservation(Guid productId, ReserveStockRequest request)
    {
        var result = await _inventoryService.ConfirmReservationAsync(productId, request);

        return Ok(result);
    }
}