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

        return _cache.GetOrCreate($"Menu_{role}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return BuildMenuForRole(role);
        });
    }

    private IEnumerable<MenuItem> BuildMenuForRole(string role)
    {
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
                        "Consent_Declaration"
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
                        "Finalise applications",
                        "Finalise applications",
                        "Finalise applications.",
                        "Application",
                        "FinaliseApplications"
                        ),
                    new MenuItem(
                        "Search",
                        "Search all records",
                        "Search all records and export results.",
                        "Application",
                        "SearchResults"
                        ),
                    new MenuItem(
                        "Download PDF form",
                        "Download PDF form",
                        "Download an eligibility form for parents to complete.",
                        "Home",
                        "FSMFormDownload"
                        ),
                    new MenuItem(
                        "Guidance",
                        "Guidance for reviewing evidence",
                        "Read guidance on how to review supporting evidence.",
                        "Home",
                        "Guidance"
                        )
                };
            case "fsmBasicVersion":
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
                        "Application",
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
            default: return Enumerable.Empty<MenuItem>();
        }
    }
}