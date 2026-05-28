using CheckYourEligibility.Admin.Domain.Enums;
using Newtonsoft.Json;
namespace CheckYourEligibility.Admin.Boundary.Responses
{
    public class EligibilityCheckReportItem
    {
        [JsonProperty("parentName")]
        public string ParentName { get; set; }

        public string NationalInsuranceNumber { get; set; } 

        public DateTime DateOfBirth { get; set; }

        [JsonProperty("checkSubmittedDate")]
        public DateTime DateCheckSubmitted { get; set; }

        public string Outcome { get; set; }

        public string CheckType { get; set; }

        public string CheckedBy { get; set; }

        public string Tier { get; set; }

        [JsonProperty("processingType")]
        public string ProcessingType { get; set; }

        //public string CheckTypeDisplay => CheckType switch
        //{
        //    CheckType.BulkChecks => "Batch",
        //    CheckType.IndividualChecks => "Individual",
        //    _ => "Unknown"
        //};
        //public string OutcomeDisplay => Outcome switch
        //{
        //    CheckEligibilityStatus.queuedForProcessing => "Queued for processing",
        //    CheckEligibilityStatus.parentNotFound => "Parent not found",
        //    CheckEligibilityStatus.eligible => "Eligible",
        //    CheckEligibilityStatus.notEligible => "Not eligible",
        //    CheckEligibilityStatus.error => "Error",
        //    CheckEligibilityStatus.notFound => "Not found",
        //    CheckEligibilityStatus.deleted => "Deleted",
        //    _ => Outcome.ToString()
        //};

        public string CheckTypeDisplay => CheckType switch
        {
            "IndividualChecks" => "Individual",
            "BulkChecks" => "Batch",
            "Individual" => "Individual",
            "Batch" => "Batch",
            _ => CheckType
        };

        public string OutcomeDisplay => Outcome switch
        {
            "eligible" => "Eligible",
            "notEligible" => "Not Eligible",
            "parentNotFound" => "Not Found",
            "notFound" => "Not Found",
            _ => Outcome
        };
    }
}