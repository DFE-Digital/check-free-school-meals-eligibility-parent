using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CheckYourEligibility.Admin.Controllers;

[Authorize]
public class BaseController : Controller
{
	protected DfeClaims? _Claims;

	private readonly IDfeSignInApiService _dfeSignInApiService;

	public BaseController(IDfeSignInApiService dfeSignInApiService)
	{
		_dfeSignInApiService = dfeSignInApiService;
	}

	public async Task GetDfeClaimsAsync()
	{
		_Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

		// Fetch roles from DfE Sign-in API
		if (_Claims.Organisation.Id != Guid.Empty && !string.IsNullOrEmpty(_Claims.User?.Id))
		{
			_Claims.Roles = await _dfeSignInApiService.GetUserRolesAsync(_Claims.User.Id, _Claims.Organisation.Id);
		}
	}

	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		await GetDfeClaimsAsync();
		ViewBag.Claims = _Claims;
		await base.OnActionExecutionAsync(context, next);
	}
}