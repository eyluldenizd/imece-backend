namespace Infrastructure.Entities;

public sealed class WeeklyMenus
{
    public long MenuId { get; set; }

    public int CompanyId { get; set; }

    public int? BranchId { get; set; }

    public string MenuCode { get; set; } = null!;

    public int Year { get; set; }

    public int Month { get; set; }

    public int WeekOfMonth { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public string? Title { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>Join-only display field from companies.</summary>
    public string? CompanyName { get; set; }

    /// <summary>Join-only display field from branches.</summary>
    public string? BranchName { get; set; }
}
