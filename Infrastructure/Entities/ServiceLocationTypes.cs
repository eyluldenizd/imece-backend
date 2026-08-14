using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class ServiceLocationTypes
{
    [DbManager.DbColumn("service_location_type_id")]
    public int ServiceLocationTypeId { get; set; }

    [DbManager.DbColumn("company_scope")]
    public string CompanyScope { get; set; } = "All";

    [DbManager.DbColumn("company_id")]
    public int? CompanyId { get; set; }

    [DbManager.DbColumn("branch_scope")]
    public string BranchScope { get; set; } = "All";

    [DbManager.DbColumn("branch_id")]
    public int? BranchId { get; set; }

    [DbManager.DbColumn("department_scope")]
    public string DepartmentScope { get; set; } = "All";

    [DbManager.DbColumn("department_id")]
    public int? DepartmentId { get; set; }

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

    /// <summary>Join-only display field from companies.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Join-only display field from branches.</summary>
    public string? BranchName { get; set; }

    /// <summary>Join-only display field from departments.</summary>
    public string? DepartmentName { get; set; }
}
