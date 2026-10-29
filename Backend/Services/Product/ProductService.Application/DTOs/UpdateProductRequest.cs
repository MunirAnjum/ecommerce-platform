using System;

namespace ProductService.Application.DTOs;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
}
