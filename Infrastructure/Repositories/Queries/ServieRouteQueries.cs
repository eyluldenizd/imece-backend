namespace Infrastructure.Queries;



public static class ServiceRouteQueries

{

    public const string GetAll = """

        SELECT

            service_route_id,

            route_name,

            departure_location,

            arrival_location,

            departure_location_id,

            arrival_location_id,

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

            departure_location_id,

            arrival_location_id,

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

            departure_location_id,

            arrival_location_id,

            route_description,

            departure_time,

            arrival_time,

            is_active,

            display_order,

            created_at

        )

        OUTPUT INSERTED.service_route_id

        VALUES

        (

            @RouteName,

            @DepartureLocation,

            @ArrivalLocation,

            @DepartureLocationId,

            @ArrivalLocationId,

            @RouteDescription,

            @DepartureTime,

            @ArrivalTime,

            @IsActive,

            @DisplayOrder,

            SYSUTCDATETIME()

        );

        """;



    public const string Update = """

        UPDATE service_routes

        SET

            route_name = @RouteName,

            departure_location = @DepartureLocation,

            arrival_location = @ArrivalLocation,

            departure_location_id = @DepartureLocationId,

            arrival_location_id = @ArrivalLocationId,

            route_description = @RouteDescription,

            departure_time = @DepartureTime,

            arrival_time = @ArrivalTime,

            is_active = @IsActive,

            display_order = @DisplayOrder,

            updated_at = SYSUTCDATETIME()

        WHERE service_route_id = @ServiceRouteId;

        """;



    public const string SoftDelete = """

        UPDATE service_routes

        SET is_active = 0, updated_at = SYSUTCDATETIME()

        WHERE service_route_id = @ServiceRouteId;

        """;

}

