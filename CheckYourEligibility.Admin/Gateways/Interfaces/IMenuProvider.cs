using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IMenuProvider
{
    IEnumerable<MenuItem> GetMenuItemsFor(string role);
}

public class MenuProvider : IMenuProvider
{
    private readonly IMemoryCache _cache;
    public MenuProvider(IMemoryCache cache) => _cache = cache;

    public IEnumerable<MenuItem> GetMenuItemsFor(string role)
    {
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
                        "MAT role links to be updated",
                        "MAT role links to be updated",
                        "MAT role links to be updated.",
                        "Home",
                        "Index"
                    )
                };
            case "fsmSchoolRole":
                return new[] {
                    new MenuItem(
                        "School role links to be updated",
                        "School role links to be updated",
                        "School role links to be updated.",
                        "Home",
                        "Index"
                    )
                };
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
                        "Download form",
                        "Download an eligibility form for parents to complete.",
                        "Home",
                        "FSMFormDownload"
                        )
                };
            case "fsmLocalAuthority":
                return new[] {
                    new MenuItem(
                        "Standard LA role links to be updated",
                        "Standard LA role links to be updated",
                        "Standard LA role links to be updated.",
                        "Home",
                        "Index"
                    )
                };
            default: return Enumerable.Empty<MenuItem>();
        }
    }
}