using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Interfaces;
public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId);

    Task<InventoryItem?> GetByIdAsync(Guid id);

    Task<List<InventoryItem>> GetAllAsync();

    Task AddAsync(InventoryItem item);
    
    Task UpdateAsync(InventoryItem item);
}
