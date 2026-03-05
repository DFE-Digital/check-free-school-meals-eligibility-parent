using CheckYourEligibility.Admin.Domain.Enums;
using Newtonsoft.Json;
namespace CheckYourEligibility.Admin.Boundary.Responses
{
    public class EligibilityCheckReportItem
    {
        [JsonProperty("LastName")]
        public string ParentName { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateCheckSubmitted { get; set; }
        public CheckType CheckType { get; set; }
        public string CheckedBy { get; set; }
    }

}
