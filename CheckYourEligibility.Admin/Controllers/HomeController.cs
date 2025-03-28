using CheckYourEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
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

    public IActionResult FSMFormDownload()
    {
        return View("FSMFormDownload");
    }
}