using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class Reservation
{
    [DbManager.DbColumn("reservation_id")]
    public int ReservationId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int? CompanyId { get; set; }

    [DbManager.DbColumn("meeting_room_id")]
    public int? MeetingRoomId { get; set; }

    [DbManager.DbColumn("room_name")]
    public string RoomName { get; set; } = null!;

    [DbManager.DbColumn("organizer_user_id")]
    public int? OrganizerUserId { get; set; }

    [DbManager.DbColumn("requester_user_id")]
    public int? RequesterUserId { get; set; }

    [DbManager.DbColumn("requester_name")]
    public string? RequesterName { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("start_time")]
    public DateTime StartTime { get; set; }

    [DbManager.DbColumn("end_time")]
    public DateTime EndTime { get; set; }

    [DbManager.DbColumn("status")]
    public string Status { get; set; } = null!;

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
