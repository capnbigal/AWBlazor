using System.Globalization;

namespace AWBlazorApp.Features.Scheduling.Services;

public static class IsoWeekHelper
{
    public static int FromDate(DateTime date)
    {
        var isoYear = ISOWeek.GetYear(date);
        var isoWeek = ISOWeek.GetWeekOfYear(date);
        return isoYear * 100 + isoWeek;
    }

    public static DateTime ToMondayUtc(int weekId)
    {
        var isoYear = weekId / 100;
        var isoWeek = weekId % 100;
        var monday = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
        return DateTime.SpecifyKind(monday, DateTimeKind.Utc);
    }

    public static string Format(int weekId) => $"{weekId / 100}-W{weekId % 100:D2}";
}
