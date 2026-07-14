namespace Infrastructure.Repositories.Queries;

public static class ReservationsQueries
{
    private const string BaseSelect = """
        SELECT
            reservation_id,
            room_name,
            organizer_user_id,
            title,
            description,
            start_time,
            end_time,
            status,
            created_at,
            updated_at
        FROM reservations
        """;

    public const string GetAll = BaseSelect + """
        
        ORDER BY start_time DESC;
        """;

    public const string GetById = BaseSelect + """
        
        WHERE reservation_id = @ReservationId;
        """;

    public const string GetByOrganizer = BaseSelect + """
        
        WHERE organizer_user_id = @OrganizerUserId
        ORDER BY start_time DESC;
        """;

    public const string GetByRoomName = BaseSelect + """
        
        WHERE room_name = @RoomName
        ORDER BY start_time DESC;
        """;

    public const string CheckOverlap = """
        SELECT
            reservation_id,
            room_name,
            organizer_user_id,
            title,
            start_time,
            end_time,
            status
        FROM reservations
        WHERE room_name = @RoomName
          AND status <> 'cancelled'
          AND reservation_id <> @ExcludeReservationId
          AND start_time < @EndTime
          AND end_time > @StartTime;
        """;

    public const string Create = """
        INSERT INTO reservations
            (room_name, organizer_user_id, title, description, start_time, end_time, status, created_at, updated_at)
        OUTPUT INSERTED.reservation_id
        VALUES
            (@RoomName, @OrganizerUserId, @Title, @Description, @StartTime, @EndTime, @Status, @CreatedAt, @UpdatedAt);
        """;

    public const string Update = """
        UPDATE reservations
        SET
            room_name = @RoomName,
            organizer_user_id = @OrganizerUserId,
            title = @Title,
            description = @Description,
            start_time = @StartTime,
            end_time = @EndTime,
            status = @Status
        WHERE reservation_id = @ReservationId;
        """;

    public const string UpdateStatus = """
        UPDATE reservations
        SET
            status = @Status
        WHERE reservation_id = @ReservationId;
        """;

    public const string Delete = """
        DELETE FROM reservations
        WHERE reservation_id = @ReservationId;
        """;
}