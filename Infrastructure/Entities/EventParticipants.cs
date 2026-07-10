using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class EventParticipants
{
    public long EventId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RegisteredAt { get; set; }

    public virtual Events Event { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
