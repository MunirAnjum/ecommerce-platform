using ProductService.Infrastructure.Configuration;
using Xunit;

namespace ProductService.UnitTests;

public class CloudinarySettingsTests
{
    [Theory]
    [InlineData("", "", "")]
    [InlineData("YOUR_CLOUDINARY_CLOUD_NAME", "123", "secret")]
    [InlineData("your_cloud_name", "", "secret")]
    [InlineData("your_cloud_name", "123", "")]
    public void IsConfigured_WithMissingOrPlaceholderValues_ShouldReturnFalse(
        string cloudName, string apiKey, string apiSecret)
    {
        var settings = new CloudinarySettings
        {
            CloudName = cloudName,
            ApiKey = apiKey,
            ApiSecret = apiSecret
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_WithValidCredentials_ShouldReturnTrue()
    {
        var settings = new CloudinarySettings
        {
            CloudName = "karakoram-harvest",
            ApiKey = "839472918471928",
            ApiSecret = "aBcdEfGhIjKlMnOpQrStUvWxYz01"
        };

        Assert.True(settings.IsConfigured);
    }
}
