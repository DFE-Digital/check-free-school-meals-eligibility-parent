using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CheckYourEligibility.Admin.Controllers;

[Authorize]
public class BaseController : Controller
{
    private readonly IDfeSignInApiService _dfeSignInApiService;

    public BaseController(IDfeSignInApiService dfeSignInApiService)
    {
        _dfeSignInApiService = dfeSignInApiService;
    }

    protected DfeClaims? _Claims;
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        // Fetch roles from DfE Sign-in API
        if (_Claims.Organisation.Id != Guid.Empty && !string.IsNullOrEmpty(_Claims.User?.Id))
        {
            _Claims.Roles = await _dfeSignInApiService.GetUserRolesAsync(_Claims.User.Id, _Claims.Organisation.Id);
        }

        ViewBag.Claims = _Claims;
        await base.OnActionExecutionAsync(context, next);
    }
}