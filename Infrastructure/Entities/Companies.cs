using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class Companies
{
    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("company_code")]
    public string CompanyCode { get; set; } = string.Empty;

    [DbManager.DbColumn("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("logo_url")]
    public string? LogoUrl { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
