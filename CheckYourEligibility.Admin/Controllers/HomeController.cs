using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers;

public class HomeController : BaseController
{
    public HomeController(IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
    {
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
}