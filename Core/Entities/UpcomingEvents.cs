namespace Core.Entities;

public sealed class UpcomingEvents
{
    public long EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}
