using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Events
{
    public long EventId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? EventType { get; set; }

    public string? Location { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    public bool IsAllDay { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<EventParticipants> EventParticipants { get; set; } = new List<EventParticipants>();

    public virtual ICollection<PhotoAlbums> PhotoAlbums { get; set; } = new List<PhotoAlbums>();
}
