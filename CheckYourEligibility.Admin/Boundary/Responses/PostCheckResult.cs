using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Responses;

public class PostCheckResult
{
    public string Id { get; set; }
    public CheckEligibilityStatus Status { get; set; }
}