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
        public CheckEligibilityStatus Outcome { get; set; }
        public CheckType CheckType { get; set; }
        public string CheckedBy { get; set; }
        public string CheckTypeDisplay => CheckType switch
        {
            CheckType.BulkChecks => "Batch",
            CheckType.IndividualChecks => "Individual",
            _ => "Unknown"
        };
        public string OutcomeDisplay => Outcome switch
        {
            CheckEligibilityStatus.queuedForProcessing => "Queued for processing",
            CheckEligibilityStatus.parentNotFound => "Parent not found",
            CheckEligibilityStatus.eligible => "Eligible",
            CheckEligibilityStatus.notEligible => "Not eligible",
            CheckEligibilityStatus.error => "Error",
            CheckEligibilityStatus.notFound => "Not found",
            CheckEligibilityStatus.deleted => "Deleted",
            _ => Outcome.ToString()
        };
    }
}