using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Domain.Entities;
public class InventoryItem
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public int AvailableQuantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private InventoryItem()
    {
        
    }

    public InventoryItem(Guid productId, int availableQuantity)
    {
        if(availableQuantity < 0)
        {
            throw new ArgumentException("Available quantity can not be negative.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if(quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        AvailableQuantity += quantity;

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (quantity > AvailableQuantity)
        {
            throw new InvalidOperationException("Insufficient available stock.");
        }

        AvailableQuantity -= quantity;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (quantity > AvailableQuantity)
        {
            throw new InvalidOperationException("Insufficient available stock.");
        }

        AvailableQuantity -= quantity;

        ReservedQuantity += quantity;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (quantity > ReservedQuantity)
        {
            throw new InvalidOperationException("Insufficient available stock.");
        }

        ReservedQuantity -= quantity;

        AvailableQuantity += quantity;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (quantity > ReservedQuantity)
        {
            throw new InvalidOperationException("Cannot confirm more stock than reserved.");
        }

        ReservedQuantity -= quantity;

        UpdatedAt = DateTime.UtcNow;
    }
}
