using Infrastructure.Repositories.Queries;

namespace Infrastructure.Queries;

public static class ServiceRouteQueries
{
    private const string SelectColumns = $"""
        SELECT
            t.service_route_id AS ServiceRouteId,
            t.route_name AS RouteName,
            t.departure_location AS DepartureLocation,
            t.arrival_location AS ArrivalLocation,
            t.departure_location_id AS DepartureLocationId,
            t.arrival_location_id AS ArrivalLocationId,
            t.route_description AS RouteDescription,
            t.departure_time AS DepartureTime,
            t.arrival_time AS ArrivalTime,
            t.is_active AS IsActive,
            t.display_order AS DisplayOrder,
            {OrganizationScopeSql.SelectColumns},
            {OrganizationScopeSql.ListNameColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_routes AS t
        {OrganizationScopeSql.ListJoins}
        """;

    public static readonly string GetAll = $"{SelectColumns} WHERE {OrganizationScopeSql.ListFilter} ORDER BY t.display_order ASC, t.route_name ASC;";

    public const string GetById = $"""
        SELECT
            t.service_route_id AS ServiceRouteId,
            t.route_name AS RouteName,
            t.departure_location AS DepartureLocation,
            t.arrival_location AS ArrivalLocation,
            t.departure_location_id AS DepartureLocationId,
            t.arrival_location_id AS ArrivalLocationId,
            t.route_description AS RouteDescription,
            t.departure_time AS DepartureTime,
            t.arrival_time AS ArrivalTime,
            t.is_active AS IsActive,
            t.display_order AS DisplayOrder,
            {OrganizationScopeSql.SelectColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_routes AS t
        WHERE t.service_route_id = @ServiceRouteId;
        """;

    public const string Create = """
        INSERT INTO service_routes
        (
            company_scope,
            company_id,
            branch_scope,
            branch_id,
            department_scope,
            department_id,
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
            @CompanyScope,
            @CompanyId,
            @BranchScope,
            @BranchId,
            @DepartmentScope,
            @DepartmentId,
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
            company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId,
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
