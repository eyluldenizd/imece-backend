using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class ServiceLocationTypes
{
    [DbManager.DbColumn("service_location_type_id")]
    public int ServiceLocationTypeId { get; set; }

    [DbManager.DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("icon_url")]
    public string? IconUrl { get; set; }

    [DbManager.DbColumn("color_key")]
    public string? ColorKey { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
