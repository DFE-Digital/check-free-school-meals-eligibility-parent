using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using System.Numerics;

namespace CheckYourEligibility.Admin.Models
{
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
                case CheckEligibilityStatus.DwpError:
                    return "Error";
                default:
                    return status.ToString();
            }
        }

        public static string GetFsmStatusDescription(this CheckEligibilityStatus status)
        {
            return GetFsmStatusDescription(status.ToString()); 
        }
    }

}
