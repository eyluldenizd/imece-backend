namespace Core.Entities;

public sealed class CorporateApps
{
    public long AppId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public int? CorporateAppCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Category { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string CompanyScope { get; set; } = "All";
    public int? CompanyId { get; set; }
    public string BranchScope { get; set; } = "All";
    public int? BranchId { get; set; }
    public string DepartmentScope { get; set; } = "All";
    public int? DepartmentId { get; set; }
}
