namespace Infrastructure.Repositories.Queries;

public static class ServiceLocationQueries
{
    private const string BaseSelect = """
        SELECT
            sl.service_location_id,
            sl.company_id,
            sl.branch_id,
            sl.name,
            sl.service_location_type_id,
            lt.name AS type_name,
            sl.location_type,
            sl.address,
            sl.latitude,
            sl.longitude,
            sl.is_active,
            sl.created_at,
            sl.updated_at
        FROM service_locations AS sl
        LEFT JOIN service_location_types AS lt
            ON lt.service_location_type_id = sl.service_location_type_id
        """;

    public static readonly string GetAll = BaseSelect + """
        
        WHERE (@CompanyId IS NULL OR sl.company_id = @CompanyId OR sl.company_id IS NULL)
          AND (@AccessibleCompanyIds IS NULL OR sl.company_id IS NULL OR sl.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')))
        ORDER BY sl.name ASC;
        """;

    public const string GetById = BaseSelect + " WHERE sl.service_location_id = @ServiceLocationId;";

    public const string Create = """
        INSERT INTO service_locations
        (company_id, branch_id, name, service_location_type_id, location_type, address, latitude, longitude, is_active)
        OUTPUT INSERTED.service_location_id
        VALUES
        (@CompanyId, @BranchId, @Name, @ServiceLocationTypeId, @LocationType, @Address, @Latitude, @Longitude, @IsActive);
        """;

    public const string Update = """
        UPDATE service_locations
        SET
            company_id = @CompanyId,
            branch_id = @BranchId,
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
