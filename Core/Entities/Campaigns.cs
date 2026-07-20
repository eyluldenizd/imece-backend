namespace Core.Entities;

public sealed class Campaigns
{
    public long CampaignId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? TargetUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string CompanyScope { get; set; } = "All";
    public int? CompanyId { get; set; }
    public string BranchScope { get; set; } = "All";
    public int? BranchId { get; set; }
    public string DepartmentScope { get; set; } = "All";
    public int? DepartmentId { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
}
