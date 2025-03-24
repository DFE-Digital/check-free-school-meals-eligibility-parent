using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;

namespace CheckYourEligibility.Admin.Controllers
{
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
    }
}
