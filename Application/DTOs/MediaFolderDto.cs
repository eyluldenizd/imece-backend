using Core.Common.Validation;

namespace Application.DTOs;

public sealed class MediaFolderDto
{
    public long FolderId { get; set; }

    public int CompanyId { get; set; }

    public long? ParentFolderId { get; set; }

    public string? ParentFolderName { get; set; }

    public string FolderName { get; set; } = string.Empty;

    public string FolderType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? EventId { get; set; }

    public string? EventTitle { get; set; }

    public long? CoverMediaFileId { get; set; }

    public string? CoverMediaPath { get; set; }

    public bool IsPublic { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public string? CreatedByFullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateMediaFolderDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket seçilmelidir.")]
    public int CompanyId { get; set; }

    public long? ParentFolderId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Klasör adı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        2,
        ErrorMessage = "Klasör adı en az 2 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        200,
        ErrorMessage = "Klasör adı en fazla 200 karakter olabilir.")]
    public string FolderName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Klasör türü zorunludur.")]
    public string FolderType { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1000,
        ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Description { get; set; }

    public long? EventId { get; set; }

    public bool IsPublic { get; set; } = true;

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir kullanıcı seçilmelidir.")]
    public int CreatedBy { get; set; }
}

public sealed class UpdateMediaFolderDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir klasör seçilmelidir.")]
    public long FolderId { get; set; }

    public long? ParentFolderId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Klasör adı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        2,
        ErrorMessage = "Klasör adı en az 2 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        200,
        ErrorMessage = "Klasör adı en fazla 200 karakter olabilir.")]
    public string FolderName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Klasör türü zorunludur.")]
    public string FolderType { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1000,
        ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string? Description { get; set; }

    public long? EventId { get; set; }

    public long? CoverMediaFileId { get; set; }

    public bool IsPublic { get; set; }

    public bool IsActive { get; set; }
}
public sealed class MediaFolderCompanyRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }
}

public sealed class MediaFolderChildrenRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir üst klasör ID değeri gönderilmelidir.")]
    public long ParentFolderId { get; set; }
}