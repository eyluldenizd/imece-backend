namespace Application.DTOs;

public sealed class DashboardSummaryDto
{
    public int Users { get; set; }
    public int ActiveCompanies { get; set; }
    public int Branches { get; set; }
    public int Departments { get; set; }
    public int Announcements { get; set; }
    public int UpcomingEvents { get; set; }
    public int ActiveServices { get; set; }
    public int MediaFiles { get; set; }
    public int Reservations { get; set; }
    public int PublishedWeeklyMenus { get; set; }
    public IReadOnlyList<DashboardEventsByMonthDto> EventsByMonth { get; set; } = [];
    public IReadOnlyList<DashboardUsersByCompanyDto> UsersByCompany { get; set; } = [];
}

public sealed class DashboardEventsByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public sealed class DashboardUsersByCompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int Count { get; set; }
}
