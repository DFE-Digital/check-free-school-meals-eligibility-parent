namespace CheckYourEligibility.Admin.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

public sealed class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;

    public AccountController(
        ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

    [Authorize]
    [Route("/account/sign-out")]
    public async Task<IActionResult> SignOut()
    {
        return new SignOutResult(new[] { OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme });
    }
}
