using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProductService.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IRedisCacheService> _redisMock = new();
    private readonly Mock<IImageStorageService> _imageStorageMock = new();

    private ProductService.Application.Services.ProductService CreateSut()
    {
        return new ProductService.Application.Services.ProductService(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _redisMock.Object,
            _imageStorageMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithImageUrl_PersistsAndReturnsImageUrl()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var category = new Category("Dry Fruits", "Baltistan specialty fruits");
        _categoryRepoMock.Setup(x => x.GetByIdAsync(catId)).ReturnsAsync(category);

        Product? capturedProduct = null;
        _productRepoMock.Setup(x => x.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new CreateProductRequest
        {
            Name = "Halman Apricot",
            Description = "A-grade dried fruit",
            Price = 1250m,
            CategoryId = catId,
            ImageUrl = "https://res.cloudinary.com/demo/image/upload/sample.jpg",
            ImagePublicId = "sample"
        };

        // Act
        var result = await sut.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ImageUrl, result.ImageUrl);
        Assert.NotNull(capturedProduct);
        Assert.Equal(request.ImageUrl, capturedProduct!.ImageUrl);
        Assert.Equal(request.ImagePublicId, capturedProduct!.ImagePublicId);
    }

    [Fact]
    public async Task UpdateAsync_WhenImageReplaced_CleansUpOldImageFromCloudinary()
    {
        // Arrange
        var prodId = Guid.NewGuid();
        var oldPublicId = "old_public_id";
        var existingProduct = new Product(
            "Old Apricot", "Old desc", 1000m, Guid.NewGuid(),
            "https://res.cloudinary.com/demo/image/upload/old.jpg", oldPublicId);

        _productRepoMock.Setup(x => x.GetByIdAsync(prodId)).ReturnsAsync(existingProduct);
        _imageStorageMock.Setup(x => x.DeleteImageAsync(oldPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();
        var newPublicId = "new_public_id";
        var request = new UpdateProductRequest
        {
            Name = "New Apricot",
            Description = "New desc",
            Price = 1100m,
            ImageUrl = "https://res.cloudinary.com/demo/image/upload/new.jpg",
            ImagePublicId = newPublicId
        };

        // Act
        var result = await sut.UpdateAsync(prodId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ImageUrl, result!.ImageUrl);
        _imageStorageMock.Verify(x => x.DeleteImageAsync(oldPublicId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
