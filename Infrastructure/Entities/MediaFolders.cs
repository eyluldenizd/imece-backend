using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class MediaFolders
{
    [DbManager.DbColumn("folder_id")]
    public long FolderId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("parent_folder_id")]
    public long? ParentFolderId { get; set; }

    [DbManager.DbColumn("folder_name")]
    public string FolderName { get; set; } = string.Empty;

    [DbManager.DbColumn("folder_type")]
    public string FolderType { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("event_id")]
    public long? EventId { get; set; }

    [DbManager.DbColumn("cover_media_file_id")]
    public long? CoverMediaFileId { get; set; }

    [DbManager.DbColumn("is_public")]
    public bool IsPublic { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_by")]
    public int CreatedBy { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}