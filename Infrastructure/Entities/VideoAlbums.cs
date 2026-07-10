using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class VideoAlbums
{
    public int VideoAlbumId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Videos> Videos { get; set; } = new List<Videos>();
}
