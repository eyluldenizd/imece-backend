using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class MediaFiles
{
    [DbManager.DbColumn("media_file_id")]
    public long MediaFileId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("folder_id")]
    public long? FolderId { get; set; }

    [DbManager.DbColumn("media_type")]
    public string MediaType { get; set; } = string.Empty;

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("original_file_name")]
    public string OriginalFileName { get; set; } = string.Empty;

    [DbManager.DbColumn("stored_file_name")]
    public string StoredFileName { get; set; } = string.Empty;

    [DbManager.DbColumn("file_extension")]
    public string FileExtension { get; set; } = string.Empty;

    [DbManager.DbColumn("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [DbManager.DbColumn("relative_path")]
    public string RelativePath { get; set; } = string.Empty;

    [DbManager.DbColumn("thumbnail_path")]
    public string? ThumbnailPath { get; set; }

    [DbManager.DbColumn("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [DbManager.DbColumn("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [DbManager.DbColumn("document_number")]
    public string? DocumentNumber { get; set; }

    [DbManager.DbColumn("document_version")]
    public string? DocumentVersion { get; set; }

    [DbManager.DbColumn("effective_date")]
    public DateOnly? EffectiveDate { get; set; }

    [DbManager.DbColumn("expiry_date")]
    public DateOnly? ExpiryDate { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("uploaded_by")]
    public int UploadedBy { get; set; }

    [DbManager.DbColumn("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}