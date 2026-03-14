using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IMenuProvider
{
    IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims);
}

public class MenuProvider : IMenuProvider
{
    private readonly IMemoryCache _cache;
    public MenuProvider(IMemoryCache cache) => _cache = cache;

    public IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims)
    {
        if (claims == null || !claims.Roles.Any())
        {
            return Array.Empty<MenuItem>();
        }
        var role = claims.Roles[0].Code;        

        // OTG change: School menus depend on Local Authority settings.
        // Include LA code in cache key so different schools can receive different menus.
        var laCode = claims.Organisation?.LocalAuthority?.Code ?? "none";

        var cacheKey = role == "fsmSchoolRole"
            ? $"Menu_{role}_{laCode}"
            : $"Menu_{role}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            // Change: shorter cache lifetime so menu reflects LA setting updates sooner
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            return BuildMenuForRole(role, laCode);
        });
    }

    private IEnumerable<MenuItem> BuildMenuForRole(string role, string? laCode)
    {
        LocalAuthoritySettingsResponse? localAuthoritySettingsResponse = null;

        _cache.TryGetValue($"LocalAuthoritySettings_{laCode}", out localAuthoritySettingsResponse);

        switch (role)
        {
            case "fsmMATRole":
                return new[] {
                    new MenuItem(
                        "Home",
                        "Home",
                        "Dashboard",
                        "Home",
                        ""
                        ),
                    new MenuItem(
                        "Run a check",
                        "Run a check for one parent or guardian",
                        "Run an eligibility check for one parent or guardian.",
                        "Check",
                        "Enter_Details"
                        ),
                    new MenuItem(
                        "Run batch check",
                        "Run a batch check",
                        "Run an eligibility check for multiple parents or guardians.",
                        "BulkCheck",
                        "Bulk_Check"
                        ),
                    new MenuItem(
                        "Pending applications",
                        "Pending applications",
                        "Check eligibility for children not found in the system.",
                        "Application",
                        "PendingApplications"
                        ),
                    new MenuItem(
                        "Search",
                        "Search all records",
                        "Search all records and export results.",
                        "Application",
                        "SearchResults"
                        ),
                    new MenuItem(
                        "Guidance",
                        "Guidance for reviewing evidence",
                        "Read guidance on how to review supporting evidence.",
                        "Home",
                        "Guidance"
                        )
                };
            case "fsmSchoolRole":
                // Change: school review tiles are controlled by Local Authority settings
                var schoolCanReviewEvidence = localAuthoritySettingsResponse?.SchoolCanReviewEvidence ?? false;

                var schoolMenuItems = new List<MenuItem>
                {
                    new MenuItem(
                        "Run a check",
                        "Run a check for one parent or guardian",
                        "Run an eligibility check for one parent or guardian.",
                        "Check",
                        "Consent_Declaration"
                        ),
                    new MenuItem(
                        "Run batch check",
                        "Run a batch check",
                        "Run an eligibility check for multiple parents or guardians.",
                        "BulkCheck",
                        "Bulk_Check"
                        )
                };

                if (schoolCanReviewEvidence)
                {
                    schoolMenuItems.Add(
                        new MenuItem(
                            "Pending applications",
                            "Pending applications",
                            "Check eligibility for children not found in the system.",
                            "Application",
                            "PendingApplications"
                        ));

                    schoolMenuItems.Add(
                        new MenuItem(
                            "Finalise applications",
                            "Finalise applications",
                            "Finalise applications.",
                            "Application",
                            "FinaliseApplications"
                        ));
                }

                schoolMenuItems.Add(
                    new MenuItem(
                        "Search",
                        "Search all records",
                        "Search all records and export results.",
                        "Application",
                        "SearchResults"
                    ));

                schoolMenuItems.Add(
                    new MenuItem(
                        "Download PDF form",
                        "Download PDF form",
                        "Download an eligibility form for parents to complete.",
                        "Home",
                        "FSMFormDownload"
                    ));

                if (schoolCanReviewEvidence)
                {
                    schoolMenuItems.Add(
                        new MenuItem(
                            "Guidance",
                            "Guidance for reviewing evidence",
                            "Read guidance on how to review supporting evidence.",
                            "Home",
                            "Guidance"
                        ));
                }

                return schoolMenuItems;
            case "fsmBasicVersion":
                return new[] {
                    new MenuItem(
                        "Run a check",
                        "Run a check for one parent or guardian",
                        "Run an eligibility check for one parent or guardian.",
                        "Check",
                        "Enter_Details_Basic"
                        ),
                    new MenuItem(
                        "Run batch check",
                        "Run a batch check",
                        "Run an eligibility check for multiple parents or guardians.",
                        "BulkCheckFsmBasic",
                        "Bulk_Check_FSMB"
                        ),
                    new MenuItem(
                        "Reports",
                        "Reports",
                        "Generate reports and view recent checks carried out using this service.",
                        "Check",
                        "Reports"
                        ),
                    new MenuItem(
                        "Guidance",
                        "Guidance",
                        "Read guidance on using this service, reviewing evidence and completing checks for parents claiming asylum.",
                        "Home",
                        "Guidance_Basic"
                        ),
                    new MenuItem(
                        "Download PDF form",
                        "Download PDF form",
                        "Download an eligibility form for parents to complete.",
                        "Home",
                        "FSMFormDownload"
                        )
                };
            case "fsmLocalAuthority":
                return new[] {
                    new MenuItem(
                        "Run a check",
                        "Run a check for one parent or guardian",
                        "Run an eligibility check for one parent or guardian.",
                        "Check",
                        "Enter_Details"
                        ),
                    new MenuItem(
                        "Run batch check",
                        "Run a batch check",
                        "Run an eligibility check for multiple parents or guardians.",
                        "BulkCheck",
                        "Bulk_Check"
                        ),
                    new MenuItem(
                        "Pending applications",
                        "Pending applications",
                        "Check eligibility for children not found in the system.",
                        "Application",
                        "PendingApplications"
                        ),
                    new MenuItem(
                        "Search",
                        "Search all records",
                        "Search all records and export results.",
                        "Application",
                        "SearchResults"
                        ),
                    new MenuItem(
                        "Guidance",
                        "Guidance for reviewing evidence",
                        "Read guidance on how to review supporting evidence.",
                        "Home",
                        "Guidance"
                        )
                };
            default: return Enumerable.Empty<MenuItem>();
        }
    }
}