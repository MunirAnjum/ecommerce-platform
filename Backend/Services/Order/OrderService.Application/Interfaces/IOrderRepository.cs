using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces;
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);

    Task<List<Order>> GetByUserIdAsync(Guid userId);

    Task<List<Order>> GetAllAsync();

    Task AddAsync (Order order);

    Task UpdateAsync (Order order);
}
