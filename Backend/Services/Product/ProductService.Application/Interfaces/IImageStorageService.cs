using ProductService.Application.DTOs;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResultDto> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
