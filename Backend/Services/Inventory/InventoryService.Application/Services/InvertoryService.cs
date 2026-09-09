using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Services;
public class InvertoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InvertoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryResponse> CreateAsync(CreateInventoryRequest request)
    {
        if(request.ProductId == Guid.Empty)
        {
            throw new InvalidOperationException("ProductId is required.");
        }

        if(request.InitialQuantity < 0)
        {
            throw new InvalidOperationException("Initial quantity can not be negative.");
        }

        var existing = await _inventoryRepository.GetByProductIdAsync(request.ProductId);

        if(existing != null)
        {
            throw new InvalidOperationException("Inventory already exist for this product.");
        }

        var item = new InventoryItem(request.ProductId, request.InitialQuantity);

        await _inventoryRepository.AddAsync(item);

        return MapToResponse(item);
    }

    public async Task<InventoryResponse> GetByProductIdAsync(Guid productId)
    {
        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if(item is null)
        {
            throw new KeyNotFoundException("Inventory not found.");
        }

        return MapToResponse(item);
    }

    public async Task<List<InventoryResponse>> GetAllAsync()
    {
        var items = await _inventoryRepository.GetAllAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<InventoryResponse> AddStockAsync(Guid productId, UpdateStockQuantity request)
    {
        if(request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if( item is null)
        {
            throw new KeyNotFoundException("Inventory not found.");
        }

        item.AddStock(request.Quantity);    

        await _inventoryRepository.UpdateAsync(item);

        return MapToResponse(item);
    }

    public async Task<InventoryResponse> ReserveStockAsync(Guid productId, ReserveStockRequest request)
    {
        if(request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity can not be zero.");
        }

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if(item is null)
        {
            throw new KeyNotFoundException("Inventory not found.");
        }

        item.ReserveStock(request.Quantity);

        await _inventoryRepository.UpdateAsync(item);

        return MapToResponse(item);
    }

    public async Task<InventoryResponse> ReleaseStockAsync(Guid productId, ReserveStockRequest request)
    {
        if(request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var item = await _inventoryRepository.GetByProductIdAsync(productId);

        if (item is null)
        {
            throw new KeyNotFoundException("Inventory not found.");
        }

        item.ReleaseReservation(request.Quantity);

        await _inventoryRepository.UpdateAsync(item);

        return MapToResponse(item);

    }

    public async Task<InventoryResponse> ConfirmReservationAsync(Guid productId, ReserveStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        var item =
            await _inventoryRepository.GetByProductIdAsync(productId);

        if (item is null)
        {
            throw new KeyNotFoundException(
                "Inventory not found.");
        }

        item.ConfirmReservation(request.Quantity);

        await _inventoryRepository.UpdateAsync(item);

        return MapToResponse(item);
    }
   
    private static InventoryResponse MapToResponse(InventoryItem item)
    {
        return new InventoryResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            AvailableQuantity = item.AvailableQuantity,
            ReservedQuantity = item.ReservedQuantity,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}