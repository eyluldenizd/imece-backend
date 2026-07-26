namespace Application.Common.ListQuery;

public static class EventLifecycleHelper
{
    public static string GetStatus(DateTime startDateTime, DateTime endDateTime, DateTime? now = null)
    {
        var current = now ?? DateTime.Now;
        if (current < startDateTime)
        {
            return "upcoming";
        }

        if (current > endDateTime)
        {
            return "completed";
        }

        return "ongoing";
    }
}
