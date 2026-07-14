/* namespace Infrastructure.Entities;

public sealed class Reservation
{
    public long ReservationId { get; set; }
    public string RoomName { get; set; } = default!;
    public long OrganizerUserId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}*/
/*
using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class Reservation
{
    [DbManager.DbColumn("reservation_id")]
    public long ReservationId { get; set; }

    [DbManager.DbColumn("room_name")]
    public string RoomName { get; set; } = null!;

    [DbManager.DbColumn("organizer_user_id")]
    public long OrganizerUserId { get; set; }

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
}*/
using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class Reservation
{
    [DbManager.DbColumn("reservation_id")]
    public int ReservationId { get; set; }

    [DbManager.DbColumn("room_name")]
    public string RoomName { get; set; } = null!;

    [DbManager.DbColumn("organizer_user_id")]
    public int OrganizerUserId { get; set; }

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