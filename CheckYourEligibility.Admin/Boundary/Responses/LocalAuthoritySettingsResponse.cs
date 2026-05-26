namespace CheckYourEligibility.Admin.Boundary.Responses;

public sealed class LocalAuthoritySettingsResponse
{
    public bool SchoolCanReviewEvidence { get; set; }

    public IEnumerable<EligibilityPolicyAssignment> EligibilityPolicies { get; set; }
}

public sealed class EligibilityPolicyAssignment
{
    public string CheckType { get; set; }
    public string EligibilityCriteria { get; set; }
}