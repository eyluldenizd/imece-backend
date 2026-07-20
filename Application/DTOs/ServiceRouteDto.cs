namespace Application.DTOs;



public class ServiceRouteDto

{

    public long ServiceRouteId { get; set; }



    public string RouteName { get; set; } = null!;



    public string DepartureLocation { get; set; } = null!;



    public string ArrivalLocation { get; set; } = null!;



    public long? DepartureLocationId { get; set; }



    public long? ArrivalLocationId { get; set; }



    public string? RouteDescription { get; set; }



    public string? DepartureTime { get; set; }



    public string? ArrivalTime { get; set; }



    public bool IsActive { get; set; }



    public int? DisplayOrder { get; set; }



    public DateTime CreatedAt { get; set; }



    public DateTime? UpdatedAt { get; set; }



    public IReadOnlyList<ServiceRouteStopDto> Stops { get; set; } = [];

}



public sealed class CreateServiceRouteDto

{

    public string RouteName { get; set; } = string.Empty;



    public string? DepartureLocation { get; set; }



    public string? ArrivalLocation { get; set; }



    public long? DepartureLocationId { get; set; }



    public long? ArrivalLocationId { get; set; }



    public string? RouteDescription { get; set; }



    public string? DepartureTime { get; set; }



    public string? ArrivalTime { get; set; }



    public bool IsActive { get; set; } = true;



    public int? DisplayOrder { get; set; }



    public IReadOnlyList<ServiceRouteStopInputDto>? Stops { get; set; }

}



public sealed class UpdateServiceRouteDto

{

    public long ServiceRouteId { get; set; }



    public string RouteName { get; set; } = string.Empty;



    public string? DepartureLocation { get; set; }



    public string? ArrivalLocation { get; set; }



    public long? DepartureLocationId { get; set; }



    public long? ArrivalLocationId { get; set; }



    public string? RouteDescription { get; set; }



    public string? DepartureTime { get; set; }



    public string? ArrivalTime { get; set; }



    public bool IsActive { get; set; }



    public int? DisplayOrder { get; set; }



    public IReadOnlyList<ServiceRouteStopInputDto>? Stops { get; set; }

}

