using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Controllers;

public class HomeController : BaseController
{
    private readonly ILocalAuthoritySettingsGateway _localAuthoritySettingsGateway;
    private readonly IMemoryCache _cache;

    public HomeController(
        IDfeSignInApiService dfeSignInApiService,
        ILocalAuthoritySettingsGateway localAuthoritySettingsGateway,
        IMemoryCache cache) : base(dfeSignInApiService)
    {
        _localAuthoritySettingsGateway = localAuthoritySettingsGateway;
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
            Constants.CategoryTypeLA => [Constants.RoleCodeLA, Constants.RoleCodeBasic],
            Constants.CategoryTypeSchool => [Constants.RoleCodeSchool],
            Constants.CategoryTypeMAT => [Constants.RoleCodeMAT],
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

        var schoolCanReviewEvidence = await CacheAndGetSchoolCanReviewEvidence();

        var model = new HomeIndexViewModel
        {
            Claims = _Claims,
            SchoolCanReviewEvidence = schoolCanReviewEvidence
        };

        return View(model);
    }
    public IActionResult Accessibility() => View("Accessibility");

    public IActionResult Cookies() => View("Cookies");

    public IActionResult Guidance() => View("Guidance");

    public IActionResult Guidance_Redirect()
    {
        ViewData["Expand"] = "asylum-support";
        return View("Guidance");
    }

    public IActionResult Guidance_Basic()
    {
        ViewData["Directory"] = "yes";
        return View("Guidance");
    }

    public IActionResult FSMFormDownload() => View("FSMFormDownload");

    public IActionResult AsylumCheck() => View("Guidance_steps/Asylum_Check");

    public IActionResult BatchCheck() => View("Guidance_steps/Batch_Check");

    public IActionResult EvidenceGuidance() => View("Guidance_steps/Evidence_Guidance");

    private async Task<bool> CacheAndGetSchoolCanReviewEvidence()
    {
        var isSchoolUser = _Claims?.Roles?.Any(r =>
            string.Equals(r.Code, Constants.RoleCodeSchool, StringComparison.OrdinalIgnoreCase)) == true;

        if (!isSchoolUser)
        {
            return false;
        }

        var laCodeString = _Claims?.Organisation?.LocalAuthority?.Code;

        if (!int.TryParse(laCodeString, out var laCode))
        {
            return false;
        }

        var cacheKey = $"LocalAuthoritySettings_{laCode}";

        if (_cache.TryGetValue(cacheKey, out LocalAuthoritySettingsResponse? cachedSettings))
        {
            return cachedSettings?.SchoolCanReviewEvidence ?? false;
        }

        // ELIG-2661B: cache LA settings before MenuProvider builds school dashboard
        var localAuthoritySettings =
            await _localAuthoritySettingsGateway.GetLocalAuthoritySettingsAsync(laCode);

        if (localAuthoritySettings != null)
        {
            _cache.Set(cacheKey, localAuthoritySettings, TimeSpan.FromMinutes(5));
        }

        return localAuthoritySettings?.SchoolCanReviewEvidence ?? false;
    }
}