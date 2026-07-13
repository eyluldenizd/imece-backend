using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class EventParticipants
{
    [DbManager.DbColumn("event_id")]
    public long EventId { get; set; }

    [DbManager.DbColumn("user_id")]
    public int UserId { get; set; }

    [DbManager.DbColumn("status")]
    public string Status { get; set; } = string.Empty;

    [DbManager.DbColumn("registered_at")]
    public DateTime RegisteredAt { get; set; }
}