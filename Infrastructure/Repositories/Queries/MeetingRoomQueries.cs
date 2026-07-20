namespace Infrastructure.Repositories.Queries;

public static class MeetingRoomQueries
{
    private const string BaseSelect = """
        SELECT
            meeting_room_id,
            company_id,
            branch_id,
            department_id,
            name,
            code,
            floor,
            capacity,
            location_description,
            features,
            is_active,
            created_at,
            updated_at
        FROM meeting_rooms
        """;

    public static readonly string GetAll = BaseSelect + """
        
        WHERE (@CompanyId IS NULL OR company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')))
        ORDER BY name ASC;
        """;

    public const string GetById = BaseSelect + " WHERE meeting_room_id = @MeetingRoomId;";

    public const string ExistsByCode = """
        SELECT COUNT(1)
        FROM meeting_rooms
        WHERE company_id = @CompanyId
          AND code = @Code
          AND meeting_room_id <> @ExcludeMeetingRoomId;
        """;

    public const string Create = """
        INSERT INTO meeting_rooms
        (
            company_id, branch_id, department_id, name, code, floor,
            capacity, location_description, features, is_active
        )
        OUTPUT INSERTED.meeting_room_id
        VALUES
        (
            @CompanyId, @BranchId, @DepartmentId, @Name, @Code, @Floor,
            @Capacity, @LocationDescription, @Features, @IsActive
        );
        """;

    public const string Update = """
        UPDATE meeting_rooms
        SET
            company_id = @CompanyId,
            branch_id = @BranchId,
            department_id = @DepartmentId,
            name = @Name,
            code = @Code,
            floor = @Floor,
            capacity = @Capacity,
            location_description = @LocationDescription,
            features = @Features,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE meeting_room_id = @MeetingRoomId;
        """;

    public const string SoftDelete = """
        UPDATE meeting_rooms
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE meeting_room_id = @MeetingRoomId;
        """;
}
