namespace Amori.Api.Infrastructure.Storage;

/// <summary>
/// Abstraction for file storage. Will be backed by AWS S3 in production.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
    string GetPublicUrl(string fileKey);
}
