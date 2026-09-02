using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;
    
    public string Discription { get; private set; } = string.Empty;
    
    public decimal Price { get; private set; }
    
    public int StockQuantity { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreateAt { get; private set; }
    
    public DateTime? UpdateAt { get; private set; }
    
    public Guid CategoryId { get; private set; }
    
    public Category Category { get; private set; } = null!;

    public Product(string name, string discription, decimal price, int stockQuantity, Guid categoryId)
    {
        Id = Guid.NewGuid();

        Name = name;
        Discription = discription;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;

        IsActive = true;
        CreateAt = DateTime.UtcNow;
    }

    public void Update(string name, string discription, decimal price)
    {
        Name = name;
        Discription = discription;
        Price= price;

        UpdateAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdateAt= DateTime.UtcNow;
    }
}

