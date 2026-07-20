namespace Application.Common.Storage;

public sealed class StoredFileResult
{
    public required string StoredFileName { get; init; }

    public required string PublicRelativeUrl { get; init; }

    public required string FileExtension { get; init; }
}
