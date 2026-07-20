namespace Core.Entities;

public sealed class Services
{
    public long ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
