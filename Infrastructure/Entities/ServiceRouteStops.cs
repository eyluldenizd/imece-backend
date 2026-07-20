using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class ServiceRouteStops
{
    [DbManager.DbColumn("service_route_stop_id")]
    public long ServiceRouteStopId { get; set; }

    [DbManager.DbColumn("service_route_id")]
    public long ServiceRouteId { get; set; }

    [DbManager.DbColumn("service_location_id")]
    public long ServiceLocationId { get; set; }

    [DbManager.DbColumn("stop_order")]
    public int StopOrder { get; set; }

    [DbManager.DbColumn("arrival_time")]
    public TimeSpan? ArrivalTime { get; set; }

    [DbManager.DbColumn("departure_time")]
    public TimeSpan? DepartureTime { get; set; }

    [DbManager.DbColumn("notes")]
    public string? Notes { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }
}
