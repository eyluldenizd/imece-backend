using Application.Common.Storage;

namespace ImeceWebAPI.Services.Storage;

public static class StoragePathBuilder
{
    public static string BuildRelativeFolder(
        StorageScopeType scopeType,
        StorageFeatureType featureType,
        int? companyId,
        string? companySlug,
        DateTime utcNow)
    {
        var featureFolder = GetFeatureFolderName(featureType);
        var yearMonth = $"{utcNow:yyyy}/{utcNow:MM}";

        if (scopeType == StorageScopeType.Global)
        {
            return $"uploads/global/{featureFolder}/{yearMonth}";
        }

        if (!companyId.HasValue)
        {
            throw new InvalidOperationException("Şirket kapsamlı depolama için companyId zorunludur.");
        }

        var slug = FileNameSanitizer.SanitizeCompanySlug(companySlug, companyId.Value);
        return $"uploads/companies/{companyId.Value}-{slug}/{featureFolder}/{yearMonth}";
    }

    public static string BuildPublicRelativeUrl(string relativeFolder, string storedFileName)
    {
        var normalizedFolder = relativeFolder.Trim('/').Replace('\\', '/');
        return $"/{normalizedFolder}/{storedFileName}";
    }

    private static string GetFeatureFolderName(StorageFeatureType featureType) =>
        featureType switch
        {
            StorageFeatureType.Announcement => "announcements",
            StorageFeatureType.Event => "events",
            StorageFeatureType.Campaign => "campaigns",
            StorageFeatureType.Gallery => "gallery",
            StorageFeatureType.ECard => "e-cards",
            StorageFeatureType.Media => "media",
            StorageFeatureType.Document => "documents",
            StorageFeatureType.Temporary => "temporary",
            _ => featureType.ToString().ToLowerInvariant()
        };
}
