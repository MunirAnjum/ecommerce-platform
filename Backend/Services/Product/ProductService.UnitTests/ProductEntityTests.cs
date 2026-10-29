using ProductService.Domain.Entities;
using System;
using Xunit;

namespace ProductService.UnitTests;

public class ProductEntityTests
{
    [Fact]
    public void Product_Creation_WithoutImage_ShouldHaveNullImageProperties()
    {
        // Arrange & Act
        var product = new Product(
            name: "Halman Apricot",
            discription: "Sun-dried organic apricots",
            price: 1200m,
            categoryId: Guid.NewGuid());

        // Assert
        Assert.NotNull(product);
        Assert.Null(product.ImageUrl);
        Assert.Null(product.ImagePublicId);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_Creation_WithCloudinaryImage_ShouldPersistImageProperties()
    {
        // Arrange
        var imageUrl = "https://res.cloudinary.com/test-cloud/image/upload/v123/products/apricot.jpg";
        var publicId = "products/apricot";

        // Act
        var product = new Product(
            name: "Kagzi Walnut",
            discription: "Thin shelled walnut",
            price: 850m,
            categoryId: Guid.NewGuid(),
            imageUrl: imageUrl,
            imagePublicId: publicId);

        // Assert
        Assert.Equal(imageUrl, product.ImageUrl);
        Assert.Equal(publicId, product.ImagePublicId);
    }

    [Fact]
    public void Product_SetImage_ShouldUpdateImagePropertiesAndTimestamp()
    {
        // Arrange
        var product = new Product("Shehtoot", "Dried Mulberry", 1500m, Guid.NewGuid());
        var imageUrl = "https://res.cloudinary.com/test-cloud/image/upload/v123/products/shehtoot.jpg";
        var publicId = "products/shehtoot";

        // Act
        product.SetImage(imageUrl, publicId);

        // Assert
        Assert.Equal(imageUrl, product.ImageUrl);
        Assert.Equal(publicId, product.ImagePublicId);
        Assert.NotNull(product.UpdateAt);
    }

    [Fact]
    public void Product_RemoveImage_ShouldClearImageProperties()
    {
        // Arrange
        var product = new Product(
            "Almonds", "Sweet Giri", 1100m, Guid.NewGuid(),
            "https://res.cloudinary.com/test-cloud/image/upload/v123/almond.jpg", "almond");

        // Act
        product.RemoveImage();

        // Assert
        Assert.Null(product.ImageUrl);
        Assert.Null(product.ImagePublicId);
        Assert.NotNull(product.UpdateAt);
    }
}
