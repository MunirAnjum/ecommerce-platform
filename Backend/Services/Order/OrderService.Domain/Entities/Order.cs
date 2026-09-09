using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities;
public class Order
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public ICollection<OrderItem> Items { get; private set; }
        = new List<OrderItem>();

    private Order()
    {
        
    }

    public Order(Guid userId, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Process()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed order can be processed.");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped.");

        Status = OrderStatus.Shipped; 
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered.");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if(Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("The shipped or delivered order can not be calceled.");

        if (Status == OrderStatus.Cancelled)
            return;

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
