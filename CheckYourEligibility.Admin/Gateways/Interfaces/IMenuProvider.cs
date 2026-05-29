using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IMenuProvider
{
    IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims, SchoolMenuContext? schoolMenuContext = null);
}

public class MenuProvider : IMenuProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MenuProvider> _logger;

    public MenuProvider(
        IMemoryCache cache,
        ILogger<MenuProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public IEnumerable<MenuItem> GetMenuItemsFor(DfeClaims claims, SchoolMenuContext? schoolMenuContext = null)
    {
        if (claims == null || !claims.Roles.Any())
        {
            return Array.Empty<MenuItem>();
        }

        var role = claims.Roles[0].Code;

        var laCode = claims.Organisation?.LocalAuthority?.Code ?? "none";
        var establishmentId = claims.Organisation?.Urn ?? "none";

        var cacheKey = role == "fsmSchoolRole"
            ? $"Menu_{role}_{laCode}_{establishmentId}"
            : $"Menu_{role}";

        var cacheHit = _cache.TryGetValue(cacheKey, out IEnumerable<MenuItem>? cachedMenu);

        _logger.LogInformation(
            "MenuProvider request Role={Role} LA={LaCode} Est={EstablishmentId} CacheKey={CacheKey} CacheHit={CacheHit}",
            role,
            laCode,
            establishmentId,
            cacheKey,
            cacheHit);

        if (cacheHit && cachedMenu is not null)
        {
            return cachedMenu;
        }

        var menu = BuildMenuForRole(role, laCode, establishmentId, schoolMenuContext).ToArray();

        _cache.Set(cacheKey, menu, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "MenuProvider cached Role={Role} CacheKey={CacheKey} Tiles={Tiles}",
            role,
            cacheKey,
            string.Join(", ", menu.Select(x => x.MenuText)));

        return menu;
    }

    private IEnumerable<MenuItem> BuildMenuForRole(
        string role,
        string? laCode,
        string? establishmentId,
        SchoolMenuContext? schoolMenuContext)
    {
        var fsmPolicy = EligibilityCriteria.expanded; //TODO: Discuss with Jim about putting fsmPolicy as a session variable set once on login.

        switch (role)
        {
            case "fsmMATRole":
                return new[]
                {
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

                var showReviewEvidenceTiles = schoolMenuContext?.ShowReviewEvidenceTiles ?? false;

                _logger.LogInformation(
                    "School menu built LA={LaCode} Est={EstablishmentId} IsPartOfMat={IsPartOfMat} MatId={MatId} ShowTiles={ShowTiles}",
                    laCode,
                    establishmentId,
                    schoolMenuContext?.IsPartOfMat,
                    schoolMenuContext?.MatId,
                    showReviewEvidenceTiles);

                var schoolMenuItems = new List<MenuItem>
                {
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
                    )
                };

                if (showReviewEvidenceTiles)
                {
                    schoolMenuItems.Add(
                        new MenuItem(
                            "Pending applications",
                            "Pending applications",
                            "Check eligibility for children not found in the system.",
                            "Application",
                            "PendingApplications"
                        ));
                }

                schoolMenuItems.Add(
                    new MenuItem(
                        "Finalise applications",
                        "Finalise applications",
                        "Finalise applications.",
                        "Application",
                        "FinaliseApplications"
                    ));

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

                if (showReviewEvidenceTiles)
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
                return new[]
                {
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
                    "EligibilityCheckReporting",
                    "Reports"
                ),
                new MenuItem(
                    "Guidance",
                    "Guidance",
                    "Read guidance on using this service, completing checks and reviewing evidence.",
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
                var fsmLocalAuthorityItems = new List<MenuItem>
                {
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
                                    )
                };

                if (fsmPolicy == EligibilityCriteria.expanded)
                {
                    fsmLocalAuthorityItems.Add(
                    new MenuItem(
                        "Guidance",
                        "Guidance",
                        "Read guidance on using this service, completing checks and reviewing evidence.",
                        "Home",
                        "Guidance_Basic"
                    ));
                }
                else
                {
                    fsmLocalAuthorityItems.Add(
                        new MenuItem(
                        "Guidance",
                        "Guidance",
                        "Read guidance on using this service, completing checks and reviewing evidence.",
                        "Home",
                        "Guidance"
                    ));
                }

                return fsmLocalAuthorityItems;

            default:
                return Enumerable.Empty<MenuItem>();
        }
    }
}