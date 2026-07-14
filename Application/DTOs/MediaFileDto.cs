using Core.Common.Validation;

namespace Application.DTOs;

public sealed class MediaFileDto
{
    public long MediaFileId { get; set; }

    public int CompanyId { get; set; }

    public long? FolderId { get; set; }

    public string? FolderName { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? ThumbnailPath { get; set; }

    public long FileSizeBytes { get; set; }

    public int? DurationSeconds { get; set; }

    public string? DocumentNumber { get; set; }

    public string? DocumentVersion { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int UploadedBy { get; set; }

    public string? UploadedByFullName { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class UploadMediaFileDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }

    public long? FolderId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Medya türü zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        30,
        ErrorMessage = "Medya türü en fazla 30 karakter olabilir.")]
    public string MediaType { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Başlık zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Başlık en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        2000,
        ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; set; }

    public int? DurationSeconds { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        100,
        ErrorMessage = "Doküman numarası en fazla 100 karakter olabilir.")]
    public string? DocumentNumber { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        50,
        ErrorMessage = "Doküman versiyonu en fazla 50 karakter olabilir.")]
    public string? DocumentVersion { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int SortOrder { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir yükleyen kullanıcı ID değeri gönderilmelidir.")]
    public int UploadedBy { get; set; }
}

public sealed class UpdateMediaFileDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir medya dosyası ID değeri gönderilmelidir.")]
    public long MediaFileId { get; set; }

    public long? FolderId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Başlık zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Başlık en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        2000,
        ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; set; }

    public int? DurationSeconds { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        100,
        ErrorMessage = "Doküman numarası en fazla 100 karakter olabilir.")]
    public string? DocumentNumber { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        50,
        ErrorMessage = "Doküman versiyonu en fazla 50 karakter olabilir.")]
    public string? DocumentVersion { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}

public sealed class MediaFileCompanyRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }
}

public sealed class MediaFileFolderRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir klasör ID değeri gönderilmelidir.")]
    public long FolderId { get; set; }
}

public sealed class MediaFileTypeRequest
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Medya türü zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        30,
        ErrorMessage = "Medya türü en fazla 30 karakter olabilir.")]
    public string MediaType { get; set; } = string.Empty;
}

public sealed class MediaFileSearchRequest
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Arama metni zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Arama metni en fazla 255 karakter olabilir.")]
    public string SearchText { get; set; } = string.Empty;
}