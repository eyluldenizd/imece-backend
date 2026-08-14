using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class ServiceRoutes
{
    [DbManager.DbColumn("service_route_id")]
    public long ServiceRouteId { get; set; }

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

    [DbManager.DbColumn("route_name")]
    public string RouteName { get; set; } = null!;

    [DbManager.DbColumn("departure_location")]
    public string DepartureLocation { get; set; } = null!;

    [DbManager.DbColumn("arrival_location")]
    public string ArrivalLocation { get; set; } = null!;

    [DbManager.DbColumn("departure_location_id")]
    public long? DepartureLocationId { get; set; }

    [DbManager.DbColumn("arrival_location_id")]
    public long? ArrivalLocationId { get; set; }

    [DbManager.DbColumn("route_description")]
    public string? RouteDescription { get; set; }

    [DbManager.DbColumn("departure_time")]
    public TimeSpan? DepartureTime { get; set; }

    [DbManager.DbColumn("arrival_time")]
    public TimeSpan? ArrivalTime { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("display_order")]
    public int? DisplayOrder { get; set; }

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
