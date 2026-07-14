namespace Application.DTOs;

public class EventParticipantDto
{
    public long EventId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RegisteredAt { get; set; }
}