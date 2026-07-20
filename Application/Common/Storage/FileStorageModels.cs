namespace Application.Common.Storage;

public enum StorageScopeType { Global = 0, Company = 1 }
public enum StorageFeatureType { Announcement, Event, Campaign, Gallery, ECard, Media, Document, Temporary }

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
}

public sealed class FileStorageRequest
{
    public required Stream Content { get; init; }
    public required string OriginalFileName { get; init; }
    public long? ContentLength { get; init; }
    public StorageScopeType ScopeType { get; init; }
    public StorageFeatureType FeatureType { get; init; }
    public int? CompanyId { get; init; }
    public string? CompanySlug { get; init; }
}

public sealed class StoredFileResult
{
    public string StoredFileName { get; init; } = string.Empty;
    public string PublicRelativeUrl { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
}
