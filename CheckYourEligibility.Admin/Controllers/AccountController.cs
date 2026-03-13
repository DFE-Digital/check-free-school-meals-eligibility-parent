using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers;

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
        // Clear the session to remove any stored tokens or user data
        HttpContext.Session.Clear();
        return new SignOutResult(new[]
            { OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme });
    }
}