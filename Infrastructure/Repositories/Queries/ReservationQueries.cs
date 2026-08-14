namespace Infrastructure.Repositories.Queries;

public static class ReservationsQueries
{
    /// <summary>
    /// company_id is coalesced from the linked meeting room when the reservation row
    /// was created before company scoping existed (NULL company_id).
    /// </summary>
    private const string BaseSelect = """
        SELECT
            r.reservation_id,
            ISNULL(r.company_id, mr.company_id) AS company_id,
            r.meeting_room_id,
            r.room_name,
            r.organizer_user_id,
            r.requester_user_id,
            r.requester_name,
            r.title,
            r.description,
            r.start_time,
            r.end_time,
            r.status,
            r.created_at,
            r.updated_at
        FROM reservations AS r
        LEFT JOIN meeting_rooms AS mr
            ON mr.meeting_room_id = r.meeting_room_id
        """;

    private const string CompanyScopeWhere = """
        (@CompanyId IS NULL OR ISNULL(r.company_id, mr.company_id) = @CompanyId)
          AND (
                @AccessibleCompanyIds IS NULL
             OR ISNULL(r.company_id, mr.company_id) IN (
                    SELECT TRY_CAST(LTRIM(RTRIM([value])) AS INT)
                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                )
          )
        """;

    public static readonly string GetAll =
        BaseSelect +
        " WHERE " + CompanyScopeWhere +
        " ORDER BY r.start_time DESC;";

    public const string GetById = BaseSelect + """
        WHERE r.reservation_id = @ReservationId;
        """;

    public static readonly string GetByOrganizer =
        BaseSelect +
        " WHERE (" + CompanyScopeWhere + ")" +
        " AND (r.organizer_user_id = @OrganizerUserId OR r.requester_user_id = @OrganizerUserId)" +
        " ORDER BY r.start_time DESC;";

    public static readonly string GetByRoomName =
        BaseSelect +
        " WHERE (" + CompanyScopeWhere + ")" +
        " AND r.room_name = @RoomName" +
        " ORDER BY r.start_time DESC;";

    public const string CheckOverlapByRoomName = """
        SELECT reservation_id
        FROM reservations
        WHERE room_name = @RoomName
          AND status <> 'cancelled'
          AND reservation_id <> @ExcludeReservationId
          AND start_time < @EndTime
          AND end_time > @StartTime;
        """;

    public const string CheckOverlapByMeetingRoom = """
        SELECT reservation_id
        FROM reservations
        WHERE meeting_room_id = @MeetingRoomId
          AND status <> 'cancelled'
          AND reservation_id <> @ExcludeReservationId
          AND start_time < @EndTime
          AND end_time > @StartTime;
        """;

    public const string Create = """
        INSERT INTO reservations
        (
            company_id, meeting_room_id, room_name,
            organizer_user_id, requester_user_id, requester_name,
            title, description, start_time, end_time, status,
            created_at, updated_at
        )
        OUTPUT INSERTED.reservation_id
        VALUES
        (
            @CompanyId, @MeetingRoomId, @RoomName,
            @OrganizerUserId, @RequesterUserId, @RequesterName,
            @Title, @Description, @StartTime, @EndTime, @Status,
            @CreatedAt, @UpdatedAt
        );
        """;

    public const string Update = """
        UPDATE reservations
        SET
            company_id = @CompanyId,
            meeting_room_id = @MeetingRoomId,
            room_name = @RoomName,
            organizer_user_id = @OrganizerUserId,
            requester_user_id = @RequesterUserId,
            requester_name = @RequesterName,
            title = @Title,
            description = @Description,
            start_time = @StartTime,
            end_time = @EndTime,
            status = @Status,
            updated_at = SYSUTCDATETIME()
        WHERE reservation_id = @ReservationId;
        """;

    public const string UpdateStatus = """
        UPDATE reservations
        SET status = @Status, updated_at = SYSUTCDATETIME()
        WHERE reservation_id = @ReservationId;
        """;

    public const string Delete = """
        DELETE FROM reservations
        WHERE reservation_id = @ReservationId;
        """;
}
