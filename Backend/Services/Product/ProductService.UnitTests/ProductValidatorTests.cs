using ProductService.Application.DTOs;
using ProductService.Application.Validators;
using System;
using Xunit;

namespace ProductService.UnitTests;

public class ProductValidatorTests
{
    private readonly CreateProductRequestValidator _createValidator = new();
    private readonly UpdateProductRequestValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_WithValidDataAndImageUrl_ShouldPass()
    {
        var request = new CreateProductRequest
        {
            Name = "Organic Chilgoza",
            Description = "Pine nuts from Baltistan",
            Price = 4500m,
            CategoryId = Guid.NewGuid(),
            ImageUrl = "https://res.cloudinary.com/test-cloud/image/upload/v1/chilgoza.jpg",
            ImagePublicId = "chilgoza"
        };

        var result = _createValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateValidator_WithExcessiveImageUrlLength_ShouldFail()
    {
        var request = new CreateProductRequest
        {
            Name = "Organic Chilgoza",
            Description = "Pine nuts from Baltistan",
            Price = 4500m,
            CategoryId = Guid.NewGuid(),
            ImageUrl = new string('a', 1001)
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductRequest.ImageUrl));
    }

    [Fact]
    public void UpdateValidator_WithValidImageUrl_ShouldPass()
    {
        var request = new UpdateProductRequest
        {
            Name = "Updated Apricot",
            Description = "Fresh batch",
            Price = 1350m,
            ImageUrl = "https://res.cloudinary.com/test-cloud/image/upload/v2/new-apricot.webp"
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }
}
