namespace Application.DTOs;

public class EventDto
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
}