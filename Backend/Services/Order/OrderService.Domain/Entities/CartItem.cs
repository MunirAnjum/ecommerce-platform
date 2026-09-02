using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities;
public class CartItem
{
    public Guid Id { get; private set; }

    public Guid CartId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public Cart Cart { get; private set; } = null!;

    public CartItem(Guid cartId, Guid productId, int quantity)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    private CartItem() 
    {
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}

