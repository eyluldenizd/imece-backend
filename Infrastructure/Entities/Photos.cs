using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Photos
{
    public long PhotoId { get; set; }

    public int AlbumId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public string? Caption { get; set; }

    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public int SortOrder { get; set; }

    public virtual PhotoAlbums Album { get; set; } = null!;

    public virtual ICollection<PhotoAlbums> PhotoAlbums { get; set; } = new List<PhotoAlbums>();

    public virtual Users UploadedByNavigation { get; set; } = null!;
}
