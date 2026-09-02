using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            return null;

        return MapToResponse(product);
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);

        if(category is null)
        {
            throw new KeyNotFoundException(
                "Category not found.");
        }

        var product = new Product(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId
        );

        await _productRepository.AddAsync(product);

        return MapToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            return null;

        product.Update(
            request.Name,
            request.Description,
            request.Price
        );

        await _productRepository.UpdateAsync(product);
        return MapToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)

            return false;

        await _productRepository.DeleteAsync(product);

        return true;
    }

    private ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Discription,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreateAt,
            UpdatedAt = product.UpdateAt
        };
    }
}