using CheckYourEligibility.Admin.Boundary.Responses;
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

    public static string GetApplicationStatusDescription(this string status, string tier = null)
    {
        Enum.TryParse(status, out ApplicationStatus statusEnum);

        switch (statusEnum)
        {
            case ApplicationStatus.Entitled:
                return tier != null ? "Eligible " + tier : "Eligible (2025-2026)";
            case ApplicationStatus.Receiving:
                return tier != null ? "Receiving " + tier + " FSM" : "Receiving entitlement (2025-2026)";
            case ApplicationStatus.EvidenceNeeded:
                return "Evidence needed";
            case ApplicationStatus.SentForReview:
                return "Sent for review";
            case ApplicationStatus.ReviewedEntitled:
                return tier != null ? "Reviewed entitled " + tier : "Reviewed entitled (2025-2026)";
            case ApplicationStatus.ReviewedNotEntitled:
                return "Reviewed not entitled";
            case ApplicationStatus.Archived:
                return "Archived";
            default:
                return status;
        }
    }

    public static string GetApplicationStatusColor(this string status, string tier = null)
    {
        Enum.TryParse(status, out ApplicationStatus statusEnum);
        switch (statusEnum)
        {
            case ApplicationStatus.Entitled:
                return tier != null && tier == CheckEligibilityExpandedTier.expanded.ToString() ? "govuk-tag--purple" + tier : "govuk-tag--green";
            case ApplicationStatus.Receiving:
                return tier != null && tier == CheckEligibilityExpandedTier.expanded.ToString() ? "govuk-tag--teal" : "govuk-tag--teal";
            case ApplicationStatus.EvidenceNeeded:
                return "govuk-tag--yellow";
            case ApplicationStatus.SentForReview:
                return "govuk-tag--blue";
            case ApplicationStatus.ReviewedEntitled:
                return tier != null && tier == CheckEligibilityExpandedTier.expanded.ToString() ? "govuk-tag--purple " + tier : "govuk-tag--green";
            case ApplicationStatus.ReviewedNotEntitled:
                return "govuk-tag--orange";
            case ApplicationStatus.Archived:
                return "govuk-tag--grey";
            default:
                return "";
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
