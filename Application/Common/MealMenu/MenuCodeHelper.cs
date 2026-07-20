namespace Application.Common.MealMenu;

public static class MenuCodeHelper
{
    public static int GetWeekOfMonth(DateOnly date) =>
        date.Day switch
        {
            <= 7 => 1,
            <= 14 => 2,
            <= 21 => 3,
            <= 28 => 4,
            _ => 5
        };

    public static string GenerateMenuCode(int year, int month, int weekOfMonth) =>
        $"{year}{month:D2}W{weekOfMonth}";

    public static bool TryGetPeriodDates(
        int year,
        int month,
        int weekOfMonth,
        out DateOnly periodStart,
        out DateOnly periodEnd)
    {
        periodStart = default;
        periodEnd = default;

        if (month is < 1 or > 12 || weekOfMonth is < 1 or > 5)
        {
            return false;
        }

        var daysInMonth = DateTime.DaysInMonth(year, month);

        if (weekOfMonth == 5 && daysInMonth < 29)
        {
            return false;
        }

        var startDay = weekOfMonth switch
        {
            1 => 1,
            2 => 8,
            3 => 15,
            4 => 22,
            5 => 29,
            _ => 0
        };

        var endDay = weekOfMonth switch
        {
            1 => Math.Min(7, daysInMonth),
            2 => Math.Min(14, daysInMonth),
            3 => Math.Min(21, daysInMonth),
            4 => Math.Min(28, daysInMonth),
            5 => daysInMonth,
            _ => 0
        };

        periodStart = new DateOnly(year, month, startDay);
        periodEnd = new DateOnly(year, month, endDay);
        return true;
    }

    public static (int Year, int Month, int WeekOfMonth)? NextPeriod(
        int year,
        int month,
        int weekOfMonth)
    {
        if (weekOfMonth < 5
            && TryGetPeriodDates(year, month, weekOfMonth + 1, out _, out _))
        {
            return (year, month, weekOfMonth + 1);
        }

        var nextMonth = month == 12 ? 1 : month + 1;
        var nextYear = month == 12 ? year + 1 : year;

        return TryGetPeriodDates(nextYear, nextMonth, 1, out _, out _)
            ? (nextYear, nextMonth, 1)
            : null;
    }
}
