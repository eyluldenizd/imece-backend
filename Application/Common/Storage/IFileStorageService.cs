namespace Application.Common.Storage;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string storedFileName, string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath,
        CancellationToken cancellationToken = default);
}
