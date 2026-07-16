namespace Infrastructure.Repositories.Queries;

public static class UpcomingEventsQueries
{
    public const string GetAll = "SELECT event_id, title, description, event_date, location, is_active FROM upcoming_events WHERE is_active = 1 ORDER BY event_date ASC;";
}
