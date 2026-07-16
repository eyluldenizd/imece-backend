using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class ServiceRoutes
{
    [DbManager.DbColumn("service_route_id")]
    public long ServiceRouteId { get; set; }

    [DbManager.DbColumn("route_name")]
    public string RouteName { get; set; } = null!;

    [DbManager.DbColumn("departure_location")]
    public string DepartureLocation { get; set; } = null!;

    [DbManager.DbColumn("arrival_location")]
    public string ArrivalLocation { get; set; } = null!;

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
}
