using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Models;

public static class Extensions
{
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
            default:
                return status;
        }
    }

    public static string GetFsmStatusDescription(this CheckEligibilityStatus status)
    {
        return GetFsmStatusDescription(status.ToString());
    }
}