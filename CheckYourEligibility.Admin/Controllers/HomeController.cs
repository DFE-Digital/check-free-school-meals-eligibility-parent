using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Controllers;

public class HomeController : BaseController
{
    private readonly IAdminGateway _adminGateway;
    private readonly IMemoryCache _cache;

    public HomeController(
    IDfeSignInApiService dfeSignInApiService,
    ILocalAuthoritySettingsGateway localAuthoritySettingsGateway,
    ISchoolMenuContextResolver schoolMenuContextResolver,
    IAdminGateway adminGateway,
    IMemoryCache cache) : base(dfeSignInApiService, schoolMenuContextResolver, localAuthoritySettingsGateway)
    {
        _adminGateway = adminGateway;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var categoryName = _Claims?.Organisation?.Category?.Name;
        if (categoryName == null)
        {
            return View("UnauthorizedOrganization");
        }

        List<string>? requiredRoleCodes = categoryName switch
        {
            DfeSignInRoles.CategoryTypeLA => [DfeSignInRoles.RoleCodeLA, DfeSignInRoles.RoleCodeBasic],
            DfeSignInRoles.CategoryTypeSchool => [DfeSignInRoles.RoleCodeSchool],
            DfeSignInRoles.CategoryTypeMAT => [DfeSignInRoles.RoleCodeMAT],
            _ => null
        };

        if (requiredRoleCodes == null)
        {
            return View("UnauthorizedOrganization");
        }

        bool hasRequiredRole = false;
        if (_Claims.Roles is IEnumerable<dynamic> rolesEnumerable)
        {
            hasRequiredRole = rolesEnumerable.Any(r =>
                requiredRoleCodes.Any(code => code.Equals(r.Code, StringComparison.OrdinalIgnoreCase)));
        }

        if (!hasRequiredRole)
        {
            return View("UnauthorizedRole");
        }

        var schoolMenuContext = ViewBag.SchoolMenuContext as SchoolMenuContext ?? new SchoolMenuContext();
        var schoolCanReviewEvidence = schoolMenuContext.ShowReviewEvidenceTiles;
        var schoolIsPartOfMat = schoolMenuContext.IsPartOfMat;

        var model = new HomeIndexViewModel
        {
            Claims = _Claims,
            SchoolMenuContext = schoolMenuContext,
            SchoolCanReviewEvidence = schoolCanReviewEvidence,
            SchoolIsPartOfMat = schoolIsPartOfMat
        };

        return View(model);
    }


    public async Task<IActionResult> Guidance()
    {
        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

        await IsExpandedFSMEnabled();

        var schoolMenuContext = ViewBag.SchoolMenuContext as SchoolMenuContext ?? new SchoolMenuContext();
        var schoolCanReviewEvidence = schoolMenuContext.ShowReviewEvidenceTiles;
        var schoolIsPartOfMat = schoolMenuContext.IsPartOfMat;

        var model = new HomeIndexViewModel
        {
            Claims = _Claims,
            SchoolMenuContext = schoolMenuContext,
            SchoolCanReviewEvidence = schoolCanReviewEvidence,
            SchoolIsPartOfMat = schoolIsPartOfMat
        };

        return View(model);
    }

    public IActionResult Accessibility() => View("Accessibility");

    public IActionResult Cookies() => View("Cookies");

    public IActionResult FSMFormDownload() => View("FSMFormDownload");

    public IActionResult AsylumCheck() => View("Guidance_steps/Asylum_Check");

    public IActionResult BatchCheck() => View("Guidance_steps/Batch_Check");

    public IActionResult EvidenceGuidance() => View("Guidance_steps/Evidence_Guidance");

    public IActionResult Rechecks() => View("Guidance_steps/Rechecks");

    public IActionResult Expansion() => View("Guidance_steps/Expansion");
}