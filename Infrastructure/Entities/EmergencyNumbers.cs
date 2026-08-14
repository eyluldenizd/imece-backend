using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class EmergencyNumbers
{
    [DbManager.DbColumn("emergency_number_id")]
    public long EmergencyNumberId { get; set; }

    public string Name { get; set; } = null!;

    [DbManager.DbColumn("phone_number")]
    public string PhoneNumber { get; set; } = null!;

    [DbManager.DbColumn("emergency_number_category_id")]
    public int? EmergencyNumberCategoryId { get; set; }

    [DbManager.DbColumn("category_name")]
    public string? CategoryName { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("display_order")]
    public int? DisplayOrder { get; set; }

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

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Join-only display field from companies.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Join-only display field from branches.</summary>
    public string? BranchName { get; set; }

    /// <summary>Join-only display field from departments.</summary>
    public string? DepartmentName { get; set; }
}
