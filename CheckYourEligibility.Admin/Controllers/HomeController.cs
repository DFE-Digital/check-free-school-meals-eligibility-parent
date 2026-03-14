using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
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
        // Check if user belongs to an allowed organization type
        var categoryName = _Claims?.Organisation?.Category?.Name;
        if (categoryName == null)
        {
            return View("UnauthorizedOrganization");
        }

        // Determine the required roles based on organization type
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

        // Check if user has any of the required roles for their organization type
        bool hasRequiredRole = false;
        if (_Claims.Roles is IEnumerable<dynamic> rolesEnumerable)
        {
            hasRequiredRole = rolesEnumerable.Any((Func<dynamic, bool>)(r =>
                requiredRoleCodes.Any(code => code.Equals(r.Code, StringComparison.OrdinalIgnoreCase))));
        }

        if (!hasRequiredRole)
        {
            return View("UnauthorizedRole");
        }

        // ELIG-2661B: populate cached LA settings before MenuProvider builds school menus
        await CacheLocalAuthoritySettingsForSchoolUser();

        return View(_Claims);
    }

    public IActionResult Privacy()
    {
        return View("Privacy");
    }

    public IActionResult Accessibility()
    {
        return View("Accessibility");
    }

    public IActionResult Cookies()
    {
        return View("Cookies");
    }

    public IActionResult Guidance()
    {
        return View("Guidance");
    }

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

    public IActionResult FSMFormDownload()
    {
        return View("FSMFormDownload");
    }

    public IActionResult AsylumCheck()
    {
        return View("Guidance_steps/Asylum_Check");
    }

    public IActionResult BatchCheck()
    {
        return View("Guidance_steps/Batch_Check");
    }

    public IActionResult EvidenceGuidance()
    {
        return View("Guidance_steps/Evidence_Guidance");
    }

    private async Task CacheLocalAuthoritySettingsForSchoolUser()
    {
        var isSchoolUser = _Claims?.Roles?.Any(r =>
            string.Equals(r.Code, Constants.RoleCodeSchool, StringComparison.OrdinalIgnoreCase)) == true;

        if (!isSchoolUser)
        {
            return;
        }

        var laCode = _Claims?.Organisation?.LocalAuthority?.Code;
        if (string.IsNullOrWhiteSpace(laCode))
        {
            return;
        }

        var cacheKey = $"LocalAuthoritySettings_{laCode}";
        if (_cache.TryGetValue(cacheKey, out LocalAuthoritySettingsResponse? _))
        {
            return;
        }

        // ELIG-2661B: cache LA settings for the school's LA so MenuProvider can toggle school review tiles
        var localAuthoritySettingsResponse = await _localAuthoritySettingsGateway
            .GetLocalAuthoritySettingsByLaCode(laCode);

        if (localAuthoritySettingsResponse != null)
        {
            _cache.Set(
                cacheKey,
                localAuthoritySettingsResponse,
                TimeSpan.FromMinutes(5));
        }
    }
}