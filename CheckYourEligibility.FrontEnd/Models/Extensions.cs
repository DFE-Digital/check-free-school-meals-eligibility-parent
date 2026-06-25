using System.Globalization;

namespace CheckYourEligibility.FrontEnd.Models;

public static class DateTimeExtensions
{
    public static TimeZoneInfo TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

    private const string timezone = "GMT Standard Time";

    public static DateTimeOffset GetDateTimeOffsetFromString(string datetime)
    {
        DateTime raw = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None);

        DateTimeOffset offset = new DateTimeOffset(raw, TimeZoneInfo.BaseUtcOffset);
        var rules = TimeZoneInfo.GetAdjustmentRules();
        if (TimeZoneInfo.IsDaylightSavingTime(raw))
        {
            offset = new DateTimeOffset(raw.AddHours(-1), rules.First().DaylightDelta);
        }
        return offset;
    }
    public static DateTime GetLocalTime(DateTime time)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo);
    }

    public static DateTime GetUTCTime(DateTime time)
    {
        if (time.Kind != DateTimeKind.Utc)
        {
            time = TimeZoneInfo.ConvertTimeToUtc(time);
        }
        return time;
    }
    public static string ToLocalString12HourFormatReadableWithAt(this DateTime datetime)
    {
        return GetLocalTime(datetime).ToString("dd MMM yyyy 'at' h:mmtt").Replace("AM", "am").Replace("PM", "pm");
    }
}
