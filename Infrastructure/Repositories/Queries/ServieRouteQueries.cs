namespace Infrastructure.Queries;

public static class ServiceRouteQueries
{
    public const string GetAll = """
        SELECT
            service_route_id,
            route_name,
            departure_location,
            arrival_location,
            route_description,
            departure_time,
            arrival_time,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM service_routes
        ORDER BY display_order ASC, route_name ASC;
        """;


    public const string GetById = """
        SELECT
            service_route_id,
            route_name,
            departure_location,
            arrival_location,
            route_description,
            departure_time,
            arrival_time,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM service_routes
        WHERE service_route_id = @ServiceRouteId;
        """;


    public const string Create = """
        INSERT INTO service_routes
        (
            route_name,
            departure_location,
            arrival_location,
            route_description,
            departure_time,
            arrival_time,
            is_active,
            display_order,
            created_at
        )
        VALUES
        (
            @RouteName,
            @DepartureLocation,
            @ArrivalLocation,
            @RouteDescription,
            @DepartureTime,
            @ArrivalTime,
            @IsActive,
            @DisplayOrder,
            GETDATE()
        );
        """;


    public const string Update = """
        UPDATE service_routes
        SET
            route_name = @RouteName,
            departure_location = @DepartureLocation,
            arrival_location = @ArrivalLocation,
            route_description = @RouteDescription,
            departure_time = @DepartureTime,
            arrival_time = @ArrivalTime,
            is_active = @IsActive,
            display_order = @DisplayOrder,
            updated_at = GETDATE()
        WHERE service_route_id = @ServiceRouteId;
        """;


    public const string Delete = """
        DELETE FROM service_routes
        WHERE service_route_id = @ServiceRouteId;
        """;
}