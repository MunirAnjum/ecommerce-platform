using InventoryService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Interfaces;
public interface IInventoryService
{
    Task<InventoryResponse> CreateAsync(CreateInventoryRequest request);

    Task<InventoryResponse> GetByProductIdAsync(Guid productId);

    Task<List<InventoryResponse>> GetAllAsync();

    Task<InventoryResponse> AddStockAsync(Guid productId, UpdateStockQuantity request);

    Task<InventoryResponse> ReserveStockAsync(Guid productId, ReserveStockRequest request);

    Task<InventoryResponse> ReleaseStockAsync(Guid productId, ReserveStockRequest request);

    Task<InventoryResponse> ConfirmReservationAsync(Guid propductId, ReserveStockRequest request);
}