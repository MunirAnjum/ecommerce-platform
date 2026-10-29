using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    public CloudinaryImageStorageService(
        IOptions<CloudinarySettings> options,
        ILogger<CloudinaryImageStorageService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        if (_settings.IsConfigured)
        {
            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret);

            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
        }
        else
        {
            _logger.LogWarning(
                "Cloudinary credentials are not configured or are placeholder values. " +
                "Image uploads will fail until valid credentials are provided.");
        }
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null || !_settings.IsConfigured)
        {
            throw new InvalidOperationException(
                "Cloudinary service is not configured. Please configure Cloudinary:CloudName, " +
                "Cloudinary:ApiKey, and Cloudinary:ApiSecret in backend environment settings.");
        }

        var uniqueId = $"prod_{Guid.NewGuid():N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = $"karakoram-harvest/products/{uniqueId}",
            Overwrite = false,
            UniqueFilename = true,
            DisplayName = Path.GetFileNameWithoutExtension(fileName)
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        return new ImageUploadResultDto
        {
            SecureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
            PublicId = uploadResult.PublicId
        };
    }

    public async Task<bool> DeleteImageAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null || !_settings.IsConfigured || string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        try
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result == "ok")
            {
                _logger.LogInformation("Deleted Cloudinary image asset: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Failed to delete Cloudinary asset {PublicId}: {Result}", publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception deleting Cloudinary asset {PublicId}", publicId);
            return false;
        }
    }
}
