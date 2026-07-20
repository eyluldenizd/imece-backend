namespace Application.Common.Storage;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(FileStorageRequest request,
        CancellationToken cancellationToken = default);

    Task<string> SaveAsync(Stream content, string storedFileName, string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string relativePath,
        CancellationToken cancellationToken = default);
}
