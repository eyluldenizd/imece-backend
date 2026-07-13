using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Events
{
    [DbManager.DbColumn("event_id")]
    public long EventId { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }
    
    [DbManager.DbColumn("event_type")]
    public string? EventType { get; set; }

    [DbManager.DbColumn("location")]
    public string? Location { get; set; }

    [DbManager.DbColumn("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [DbManager.DbColumn("start_datetime")]
    public DateTime StartDatetime { get; set; }
    
    [DbManager.DbColumn("end_datetime")]
    public DateTime EndDatetime { get; set; }

    [DbManager.DbColumn("is_all_day")]
    public bool IsAllDay { get; set; }

    [DbManager.DbColumn("created_by")]
    public int CreatedBy { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<EventParticipants> EventParticipants { get; set; } = new List<EventParticipants>();

    public virtual ICollection<PhotoAlbums> PhotoAlbums { get; set; } = new List<PhotoAlbums>();
}
