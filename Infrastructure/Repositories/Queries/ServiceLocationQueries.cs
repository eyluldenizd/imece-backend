namespace Infrastructure.Repositories.Queries;

public static class ServiceLocationQueries
{
    private const string SelectColumns = $"""
        SELECT
            t.service_location_id AS ServiceLocationId,
            t.name AS Name,
            t.service_location_type_id AS ServiceLocationTypeId,
            lt.name AS TypeName,
            t.location_type AS LocationType,
            t.address AS Address,
            t.latitude AS Latitude,
            t.longitude AS Longitude,
            t.is_active AS IsActive,
            {OrganizationScopeSql.SelectColumns},
            {OrganizationScopeSql.ListNameColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_locations AS t
        LEFT JOIN service_location_types AS lt
            ON lt.service_location_type_id = t.service_location_type_id
        {OrganizationScopeSql.ListJoins}
        """;

    public static readonly string GetAll = $"{SelectColumns} WHERE {OrganizationScopeSql.ListFilter} ORDER BY t.name ASC;";

    public const string GetById = $"""
        SELECT
            t.service_location_id AS ServiceLocationId,
            t.name AS Name,
            t.service_location_type_id AS ServiceLocationTypeId,
            lt.name AS TypeName,
            t.location_type AS LocationType,
            t.address AS Address,
            t.latitude AS Latitude,
            t.longitude AS Longitude,
            t.is_active AS IsActive,
            {OrganizationScopeSql.SelectColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_locations AS t
        LEFT JOIN service_location_types AS lt
            ON lt.service_location_type_id = t.service_location_type_id
        WHERE t.service_location_id = @ServiceLocationId;
        """;

    public const string Create = """
        INSERT INTO service_locations
        (
            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,
            name, service_location_type_id, location_type, address, latitude, longitude, is_active
        )
        OUTPUT INSERTED.service_location_id
        VALUES
        (
            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,
            @Name, @ServiceLocationTypeId, @LocationType, @Address, @Latitude, @Longitude, @IsActive
        );
        """;

    public const string Update = """
        UPDATE service_locations
        SET
            company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId,
            name = @Name,
            service_location_type_id = @ServiceLocationTypeId,
            location_type = @LocationType,
            address = @Address,
            latitude = @Latitude,
            longitude = @Longitude,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE service_location_id = @ServiceLocationId;
        """;

    public const string SoftDelete = """
        UPDATE service_locations
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE service_location_id = @ServiceLocationId;
        """;

    public const string Delete = "DELETE FROM service_locations WHERE service_location_id = @ServiceLocationId;";
}

public static class ServiceRouteStopQueries
{
    private const string BaseSelect = """
        SELECT
            service_route_stop_id,
            service_route_id,
            service_location_id,
            stop_order,
            arrival_time,
            departure_time,
            notes,
            is_active
        FROM service_route_stops
        """;

    public const string GetByRouteId = BaseSelect + """
        
        WHERE service_route_id = @ServiceRouteId
        ORDER BY stop_order ASC;
        """;

    public const string DeleteByRouteId = """
        DELETE FROM service_route_stops
        WHERE service_route_id = @ServiceRouteId;
        """;

    public const string Create = """
        INSERT INTO service_route_stops
        (service_route_id, service_location_id, stop_order, arrival_time, departure_time, notes, is_active)
        VALUES
        (@ServiceRouteId, @ServiceLocationId, @StopOrder, @ArrivalTime, @DepartureTime, @Notes, @IsActive);
        """;
}
