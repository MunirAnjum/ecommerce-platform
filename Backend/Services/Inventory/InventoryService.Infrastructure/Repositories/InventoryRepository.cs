using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistences;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Repositories;
public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId)
    {
        return await _context.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId);
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems.OrderBy(x => x.CreatedAt).ToListAsync();
    }

    public async Task AddAsync(InventoryItem item)
    {
        await _context.InventoryItems.AddAsync(item);

        await _context.SaveChangesAsync();  
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        await _context.SaveChangesAsync();
    }
}