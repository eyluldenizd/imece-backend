using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class PhotoAlbums
{
    public int AlbumId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public long? EventId { get; set; }

    public long? CoverPhotoId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Photos? CoverPhoto { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual Events? Event { get; set; }

    public virtual ICollection<Photos> Photos { get; set; } = new List<Photos>();
}
