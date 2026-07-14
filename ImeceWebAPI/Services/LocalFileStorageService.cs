using Application.Common.Storage;

namespace ImeceWebAPI.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(Stream content, string storedFileName,
        string folder, CancellationToken cancellationToken = default)
    {
        var webRoot = GetWebRoot();
        var normalizedFolder = folder.Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        var physicalFolder = Path.Combine(webRoot, normalizedFolder);
        Directory.CreateDirectory(physicalFolder);

        var physicalPath = Path.Combine(physicalFolder, storedFileName);
        await using var output = new FileStream(physicalPath, FileMode.CreateNew,
            FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        await content.CopyToAsync(output, cancellationToken);

        return $"/{folder.Trim('/')}/{storedFileName}";
    }

    public Task DeleteAsync(string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var webRoot = Path.GetFullPath(GetWebRoot());
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(Path.Combine(webRoot, normalizedPath));

        if (!physicalPath.StartsWith(webRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Geçersiz dosya yolu.");
        }

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    private string GetWebRoot() =>
        string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
}
