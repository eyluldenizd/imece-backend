using Infrastructure.Data;

namespace Infrastructure.Repositories;

public sealed class UserEnrichedRecord
{
    [DbManager.DbColumn("user_id")]
    public int UserId { get; set; }

    [DbManager.DbColumn("azure_object_id")]
    public string AzureObjectId { get; set; } = string.Empty;

    [DbManager.DbColumn("username")]
    public string? Username { get; set; }

    [DbManager.DbColumn("email")]
    public string Email { get; set; } = string.Empty;

    [DbManager.DbColumn("full_name")]
    public string FullName { get; set; } = string.Empty;

    [DbManager.DbColumn("title")]
    public string? Title { get; set; }

    [DbManager.DbColumn("company_id")]
    public int? CompanyId { get; set; }

    [DbManager.DbColumn("company_name")]
    public string? CompanyName { get; set; }

    [DbManager.DbColumn("department_id")]
    public int? DepartmentId { get; set; }

    [DbManager.DbColumn("department_name")]
    public string? DepartmentName { get; set; }

    [DbManager.DbColumn("branch_id")]
    public int? BranchId { get; set; }

    [DbManager.DbColumn("branch_name")]
    public string? BranchName { get; set; }

    [DbManager.DbColumn("role_id")]
    public int RoleId { get; set; }

    [DbManager.DbColumn("role_name")]
    public string? RoleName { get; set; }

    [DbManager.DbColumn("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [DbManager.DbColumn("hire_date")]
    public DateOnly? HireDate { get; set; }

    [DbManager.DbColumn("phone")]
    public string? Phone { get; set; }

    [DbManager.DbColumn("photo_url")]
    public string? PhotoUrl { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
