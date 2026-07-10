namespace Application.DTOs;

public class AnnouncementDto
{
    public long AnnouncementId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public int AuthorUserId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishStart { get; set; }

    public DateTime? PublishEnd { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}