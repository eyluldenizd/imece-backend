using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Videos
{
    public long VideoId { get; set; }

    public int VideoAlbumId { get; set; }

    public string Title { get; set; } = null!;

    public string VideoSource { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public int? DurationSeconds { get; set; }

    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public int SortOrder { get; set; }

    public virtual Users UploadedByNavigation { get; set; } = null!;

    public virtual VideoAlbums VideoAlbum { get; set; } = null!;
}
