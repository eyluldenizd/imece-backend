namespace Application.DTOs;

public class ServiceRouteDto
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

public class CreateServiceRouteDto
{
    public string RouteName { get; set; } = string.Empty;
    public string DepartureLocation { get; set; } = string.Empty;
    public string ArrivalLocation { get; set; } = string.Empty;
    public string? RouteDescription { get; set; }
    public TimeSpan? DepartureTime { get; set; }
    public TimeSpan? ArrivalTime { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DisplayOrder { get; set; }
}

public sealed class UpdateServiceRouteDto : CreateServiceRouteDto
{
    public long ServiceRouteId { get; set; }
}
