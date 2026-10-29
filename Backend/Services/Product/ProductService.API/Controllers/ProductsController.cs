using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductsController(
        IProductService productService, 
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var product = await _productService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        ); 
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var product = await _productService.UpdateAsync(id, request);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);
        
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Uploads a product image directly to Cloudinary and returns the secure URL and public ID.
    /// Restricted to administrators.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromServices] IImageStorageService imageStorage,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was provided for upload." });
        }

        // Validate allowed content type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        var contentType = file.ContentType.ToLowerInvariant();
        if (!allowedTypes.Contains(contentType))
        {
            return BadRequest(new { message = "Only JPEG, PNG, and WebP images are permitted." });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Invalid file extension. Allowed extensions: .jpg, .jpeg, .png, .webp" });
        }

        // Validate file size (max 5 MB)
        const long maxSizeBytes = 5 * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            return BadRequest(new { message = "File size exceeds the 5 MB limit." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await imageStorage.UploadImageAsync(
                stream,
                file.FileName,
                file.ContentType,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Image upload failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes a product image asset from Cloudinary by its public ID.
    /// Restricted to administrators.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("images/{publicId}")]
    public async Task<IActionResult> DeleteImage(
        string publicId,
        [FromServices] IImageStorageService imageStorage,
        CancellationToken cancellationToken)
    {
        var decodedPublicId = Uri.UnescapeDataString(publicId);
        var success = await imageStorage.DeleteImageAsync(decodedPublicId, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "Failed to delete image from Cloudinary or image was not found." });
        }
        return NoContent();
    }
}
