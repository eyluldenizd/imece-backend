using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class EventParticipants
{
    [DbManager.DbColumn("event_participant_id")]
    public long EventId { get; set; }

    [DbManager.DbColumn("user_id")]
    public int UserId { get; set; }

    [DbManager.DbColumn("status")]
    public string Status { get; set; } = null!;

    [DbManager.DbColumn("registered_at")]
    public DateTime RegisteredAt { get; set; }

    public virtual Events Event { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
