namespace Application.DTOs;

public sealed class UpdateAnnouncementDto
{
    public long AnnouncementId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public int AuthorUserId { get; set; }
    public bool IsPinned { get; set; }
    public DateTime PublishStart { get; set; }
    public DateTime? PublishEnd { get; set; }
}