namespace CheckYourEligibility.Admin.ViewModels
{
    public class BulkCheckSummaryViewModel
    {
        public string FileName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int EligibleTargetedCount { get; set; }
        public int EligibleExpandedCount { get; set; }
        public int NotEligibleTotalCount { get; set; }
        public int NotFoundCount { get; set; }
        public int ErrorCount { get; set; }
        public int EligiblityEndDate { get; set; }

    }
}
