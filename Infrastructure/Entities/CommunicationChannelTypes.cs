using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class CommunicationChannelTypes
{
    [DbManager.DbColumn("communication_channel_type_id")]
    public int CommunicationChannelTypeId { get; set; }

    [DbManager.DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.DbColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.DbColumn("icon_url")]
    public string? IconUrl { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
