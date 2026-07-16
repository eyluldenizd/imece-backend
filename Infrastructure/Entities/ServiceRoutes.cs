namespace Infrastructure.Entities;

public partial class ServiceRoutes
{
    public long ServiceRouteId { get; set; }

    public string RouteName { get; set; } = null!;

    public string DepartureLocation { get; set; } = null!;

    public string ArrivalLocation { get; set; } = null!;

    public string? RouteDescription { get; set; }

    public TimeSpan? DepartureTime { get; set; }

    public TimeSpan? ArrivalTime { get; set; }

    public bool IsActive { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}