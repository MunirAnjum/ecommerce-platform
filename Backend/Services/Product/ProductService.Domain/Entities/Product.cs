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
        
    public bool IsActive { get; private set; }
    
    public DateTime CreateAt { get; private set; }
    
    public DateTime? UpdateAt { get; private set; }
    
    public Guid CategoryId { get; private set; }
    
    public Category Category { get; private set; } = null!;

    public string? ImageUrl { get; private set; }

    public string? ImagePublicId { get; private set; }

    // Private parameterless constructor for EF Core
    private Product() { }

    public Product(
        string name,
        string discription,
        decimal price,
        Guid categoryId,
        string? imageUrl = null,
        string? imagePublicId = null)
    {
        Id = Guid.NewGuid();

        Name = name;
        Discription = discription;
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        ImagePublicId = imagePublicId;
        IsActive = true;
        CreateAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string discription,
        decimal price,
        Guid? categoryId = null,
        string? imageUrl = null,
        string? imagePublicId = null)
    {
        Name = name;
        Discription = discription;
        Price = price;

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            CategoryId = categoryId.Value;
        }

        if (imageUrl is not null)
        {
            ImageUrl = imageUrl;
            ImagePublicId = imagePublicId;
        }

        UpdateAt = DateTime.UtcNow;
    }

    public void SetImage(string imageUrl, string? imagePublicId)
    {
        ImageUrl = imageUrl;
        ImagePublicId = imagePublicId;
        UpdateAt = DateTime.UtcNow;
    }

    public void RemoveImage()
    {
        ImageUrl = null;
        ImagePublicId = null;
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
        UpdateAt = DateTime.UtcNow;
    }
}
