namespace CheckYourEligibility.Admin.Models
{
    public class TieredOutcomeBasic
    {
        public ParentGuardianBasic ParentGuardian { get; set; }

        public string Status { get; set; }

        public string Tier { get; set; }

        public string? EligibilityEndDate { get; set; }
    }
}
