using CheckYourEligibility.Admin.Domain.DfeSignIn;

namespace CheckYourEligibility.Admin.ViewModels;

public sealed class HomeIndexViewModel
{
    public required DfeClaims Claims { get; init; }
    public bool SchoolCanReviewEvidence { get; init; }
}