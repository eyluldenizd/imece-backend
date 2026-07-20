namespace Application.Common.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public long MaxFileSizeBytes { get; init; } = 25 * 1024 * 1024;
}
