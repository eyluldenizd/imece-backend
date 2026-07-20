namespace Application.Common.Storage;

public sealed class FileStorageRequest
{
    public required Stream Content { get; init; }

    public required string OriginalFileName { get; init; }

    public StorageFeatureType FeatureType { get; init; } = StorageFeatureType.Media;

    public StorageScopeType ScopeType { get; init; } = StorageScopeType.Company;

    public int? CompanyId { get; init; }

    public string? CompanySlug { get; init; }

    public long? ContentLength { get; init; }
}
