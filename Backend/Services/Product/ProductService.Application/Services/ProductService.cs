using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRedisCacheService _redisCache;

    private const string AllProductsCacheKey = "products:all";

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IRedisCacheService redisCache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _redisCache = redisCache;
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

        // 2. Cache miss → get from PostgreSQL
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

        // 2. Cache miss → get from PostgreSQL
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
            request.StockQuantity,
            request.CategoryId
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

        product.Update(
            request.Name,
            request.Description,
            request.Price
        );

        await _productRepository.UpdateAsync(product);

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

        await _productRepository.DeleteAsync(product);

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
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreateAt,
            UpdatedAt = product.UpdateAt
        };
    }
}