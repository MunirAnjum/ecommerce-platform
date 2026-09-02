using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities;
public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    public Order Order { get; set; } = null!;

    private OrderItem()
    {
        
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int qunatity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = qunatity;
    }

    public void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}

