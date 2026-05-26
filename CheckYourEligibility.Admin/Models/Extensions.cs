using CheckYourEligibility.Admin.Domain.Enums;
using System.Globalization;
using System.Text;

namespace CheckYourEligibility.Admin.Models;

public static class Extensions
{

    public static string GetFsmStatusDescriptionBulkCheck(this string status, string tier = null)
    {
        Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

        switch (statusEnum)
        {
            case CheckEligibilityStatus.parentNotFound:
                return "Information does not match records";
            case CheckEligibilityStatus.eligible:
                return tier != null ? "Eligible " + tier : "Eligible";
            case CheckEligibilityStatus.notEligible:
                return "Not eligible";
            case CheckEligibilityStatus.error:
                return "Error";
            case CheckEligibilityStatus.deleted:
                return "Deleted";
            default:
                return status;
        }
    }

    public static string GetFsmStatusDescription(this string status)
    {
        Enum.TryParse(status, out CheckEligibilityStatus statusEnum);

        switch (statusEnum)
        {
            case CheckEligibilityStatus.parentNotFound:
                return "May not be entitled";
            case CheckEligibilityStatus.eligible:
                return "Entitled";
            case CheckEligibilityStatus.notEligible:
                return "Not Entitled";
            case CheckEligibilityStatus.error:
                return "Error";
            case CheckEligibilityStatus.deleted:
                return "Deleted";
            default:
                return status;
        }
    }

}
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
    public static string ToLocalString12HourFormatReadable(this DateTime datetime)
    {
        return GetLocalTime(datetime).ToString("dd MMM yyyy hh:mmtt").Replace("AM", "am").Replace("PM", "pm");
    }
}
