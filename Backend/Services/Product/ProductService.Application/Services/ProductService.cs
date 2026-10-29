using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRedisCacheService _redisCache;
    private readonly IImageStorageService? _imageStorage;

    private const string AllProductsCacheKey = "products:all";

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IRedisCacheService redisCache,
        IImageStorageService? imageStorage = null)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _redisCache = redisCache;
        _imageStorage = imageStorage;
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"product:{id}";

        // 1. Check Redis
        var cachedProduct =
            await _redisCache.GetAsync<ProductResponse>(cacheKey);

        if (cachedProduct is not null)
        {
            return cachedProduct;
        }

        // 2. Cache miss -> get from PostgreSQL
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            return null;

        var response = MapToResponse(product);

        // 3. Store in Redis
        await _redisCache.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(10));

        return response;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        // 1. Check Redis
        var cachedProducts =
            await _redisCache.GetAsync<List<ProductResponse>>(
                AllProductsCacheKey);

        if (cachedProducts is not null)
        {
            return cachedProducts;
        }

        // 2. Cache miss -> get from PostgreSQL
        var products = await _productRepository.GetAllAsync();

        var response =
            products.Select(MapToResponse).ToList();

        // 3. Store in Redis
        await _redisCache.SetAsync(
            AllProductsCacheKey,
            response,
            TimeSpan.FromMinutes(5));

        return response;
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request)
    {
        var category =
            await _categoryRepository.GetByIdAsync(
                request.CategoryId);

        if (category is null)
        {
            throw new KeyNotFoundException(
                "Category not found.");
        }

        var product = new Product(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.ImageUrl,
            request.ImagePublicId
        );

        await _productRepository.AddAsync(product);

        // Product list cache is now outdated
        await _redisCache.RemoveAsync(
            AllProductsCacheKey);

        return MapToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request)
    {
        var product =
            await _productRepository.GetByIdAsync(id);

        if (product is null)
            return null;

        var oldPublicId = product.ImagePublicId;

        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.ImageUrl,
            request.ImagePublicId
        );

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) product.Activate();
            else product.Deactivate();
        }

        await _productRepository.UpdateAsync(product);

        // If image was replaced successfully, clean up old Cloudinary asset
        if (!string.IsNullOrEmpty(request.ImagePublicId) &&
            !string.IsNullOrEmpty(oldPublicId) &&
            request.ImagePublicId != oldPublicId &&
            _imageStorage is not null)
        {
            try
            {
                await _imageStorage.DeleteImageAsync(oldPublicId);
            }
            catch
            {
                // Non-critical background cleanup failure
            }
        }

        // Invalidate product cache
        await _redisCache.RemoveAsync(
            $"product:{id}");

        // Invalidate product list cache
        await _redisCache.RemoveAsync(
            AllProductsCacheKey);

        return MapToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id);

        if (product is null)
            return false;

        var publicId = product.ImagePublicId;

        await _productRepository.DeleteAsync(product);

        // Clean up Cloudinary asset if one existed
        if (!string.IsNullOrEmpty(publicId) && _imageStorage is not null)
        {
            try
            {
                await _imageStorage.DeleteImageAsync(publicId);
            }
            catch
            {
                // Non-critical background cleanup failure
            }
        }

        // Remove deleted product from Redis
        await _redisCache.RemoveAsync(
            $"product:{id}");

        // Product list is also outdated
        await _redisCache.RemoveAsync(
            AllProductsCacheKey);

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
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreateAt,
            UpdatedAt = product.UpdateAt
        };
    }
}
