using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers;

[AllowAnonymous]
public class StartController : Controller
{
    private readonly IConfiguration _configuration;

    public StartController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true &&
            _configuration.GetValue<bool>("RedirectAuthenticatedUsersFromStartPage"))
        {
            return Redirect("/home");
        }

        return View();
    }
}
