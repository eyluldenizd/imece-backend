namespace Application.Common.Storage;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        FileStorageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string publicRelativeUrl,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string publicRelativeUrl,
        CancellationToken cancellationToken = default);
}
