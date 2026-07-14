using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class PhotoAlbums
{
    [DbManager.DbColumn("album_id")]
    public int AlbumId { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("event_id")]
    public long? EventId { get; set; }

    [DbManager.DbColumn("cover_photo_id")]
    public long? CoverPhotoId { get; set; }

    [DbManager.DbColumn("created_by")]
    public int CreatedBy { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public virtual Photos? CoverPhoto { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual Events? Event { get; set; }

    public virtual ICollection<Photos> Photos { get; set; } = new List<Photos>();
}
