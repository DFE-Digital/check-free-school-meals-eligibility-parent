using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Boundary.Responses
{
    public class EligibilityCheckReportHistoryItem
    {
        [JsonProperty("reportID")]
        public string ReportID { get; set; }
        public DateTime ReportGeneratedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GeneratedBy { get; set; }
        public int NumberOfResults { get; set; }
        public string Status { get; set; }
    }
}