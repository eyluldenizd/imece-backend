using Application.Common.Storage;
using ImeceWebAPI.Services.Storage;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Services;

public sealed class WwwRootFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageOptions _options;

    public WwwRootFileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<StoredFileResult> SaveAsync(
        FileStorageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request.Content);

        if (request.ContentLength.HasValue &&
            request.ContentLength.Value > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"Dosya boyutu {_options.MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.");
        }

        var extension = FileNameSanitizer.GetSafeExtension(request.OriginalFileName);
        var storedFileName = FileNameSanitizer.CreateStoredFileName(extension);
        var utcNow = DateTime.UtcNow;
        var relativeFolder = StoragePathBuilder.BuildRelativeFolder(
            request.ScopeType,
            request.FeatureType,
            request.CompanyId,
            request.CompanySlug,
            utcNow);

        var webRoot = GetWebRoot();
        var normalizedFolder = relativeFolder
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        var physicalFolder = Path.Combine(webRoot, normalizedFolder);
        Directory.CreateDirectory(physicalFolder);

        var physicalPath = Path.Combine(physicalFolder, storedFileName);
        await using var output = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);
        await request.Content.CopyToAsync(output, cancellationToken);

        return new StoredFileResult
        {
            StoredFileName = storedFileName,
            PublicRelativeUrl = StoragePathBuilder.BuildPublicRelativeUrl(
                relativeFolder,
                storedFileName),
            FileExtension = extension
        };
    }

    public async Task<string> SaveAsync(
        Stream content,
        string storedFileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var safeName = Path.GetFileName(storedFileName);
        if (!string.Equals(safeName, storedFileName, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(safeName))
        {
            throw new InvalidOperationException("Geçersiz dosya adı.");
        }

        var relativeFolder = folder.Replace('\\', '/').Trim('/');
        var relativeUrl = StoragePathBuilder.BuildPublicRelativeUrl(relativeFolder, safeName);
        var physicalPath = ResolvePhysicalPath(relativeUrl);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await using var output = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, 81920, FileOptions.Asynchronous);
        await content.CopyToAsync(output, cancellationToken);
        return relativeUrl;
    }

    public Task DeleteAsync(
        string publicRelativeUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var physicalPath = ResolvePhysicalPath(publicRelativeUrl);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string publicRelativeUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var physicalPath = ResolvePhysicalPath(publicRelativeUrl);
        return Task.FromResult(File.Exists(physicalPath));
    }

    private string ResolvePhysicalPath(string publicRelativeUrl)
    {
        if (string.IsNullOrWhiteSpace(publicRelativeUrl))
        {
            throw new InvalidOperationException("Geçersiz dosya yolu.");
        }

        // Public URLs use a leading slash (e.g. /uploads/...). On Windows that is
        // treated as rooted unless normalized first.
        var normalizedPath = publicRelativeUrl
            .Replace('\\', '/')
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedPath))
        {
            throw new InvalidOperationException("Mutlak dosya yolları desteklenmiyor.");
        }

        var webRoot = Path.GetFullPath(GetWebRoot());
        var physicalPath = Path.GetFullPath(Path.Combine(webRoot, normalizedPath));

        if (!physicalPath.StartsWith(
                webRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Geçersiz dosya yolu.");
        }

        return physicalPath;
    }

    private string GetWebRoot()
    {
        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        Directory.CreateDirectory(webRoot);
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));

        return webRoot;
    }
}
