using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class ECards
{
    [DbManager.DbColumn("e_card_id")]
    public long ECardId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [DbManager.DbColumn("card_type")]
    public string? CardType { get; set; }

    [DbManager.DbColumn("image_url")]
    public string? ImageUrl { get; set; }

    [DbManager.DbColumn("redirect_url")]
    public string? RedirectUrl { get; set; }

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
}
