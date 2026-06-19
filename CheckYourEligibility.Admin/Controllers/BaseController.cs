using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Controllers;

[Authorize]
public class BaseController : Controller
{
    protected DfeClaims? _Claims;

    private readonly IDfeSignInApiService _dfeSignInApiService;
    protected readonly ISchoolMenuContextResolver _schoolMenuContextResolver;
    private readonly ILocalAuthoritySettingsGateway _localAuthoritySettingsGateway;

    public BaseController(
    IDfeSignInApiService dfeSignInApiService,
    ISchoolMenuContextResolver schoolMenuContextResolver,
    ILocalAuthoritySettingsGateway localAuthoritySettingsGateway)
    {
        _dfeSignInApiService = dfeSignInApiService;
        _schoolMenuContextResolver = schoolMenuContextResolver;
        _localAuthoritySettingsGateway = localAuthoritySettingsGateway;
    }

    protected (int, OrganisationCategory) GetOrganisationIdandType()
    {
        int organisationId = 0;
        OrganisationCategory organisationType = OrganisationCategory.None;
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        switch (_Claims.Organisation.Category.Id)
        {
            case OrganisationCategory.LocalAuthority:
                organisationType = OrganisationCategory.LocalAuthority;
                organisationId = Int32.Parse(_Claims.Organisation.EstablishmentNumber);
                break;
            case OrganisationCategory.MultiAcademyTrust:
                organisationType = OrganisationCategory.MultiAcademyTrust;
                organisationId = Int32.Parse(_Claims.Organisation.Uid);
                break;
            case OrganisationCategory.Establishment:
                organisationType = OrganisationCategory.Establishment;
                organisationId = Int32.Parse(_Claims.Organisation.Urn);
                break;
        };

        return (organisationId, organisationType);
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
        ViewBag.SchoolMenuContext = await _schoolMenuContextResolver.ResolveAsync(_Claims);
        await base.OnActionExecutionAsync(context, next);
    }

    /// <summary>
    /// Retrieve the current Free School Meals policy for the user's local authority.  Retreived from session if available, otherwise from the Local Authority API.
    /// </summary>
    public async Task<EligibilityPolicyAssignment> GetFreeSchoolMealsPolicy()
    {
        EligibilityPolicyAssignment policy;

        try
        {
            var laID = Convert.ToInt32(_Claims.Organisation.EstablishmentNumber);
            //For school use their LA's tier policy setting.
            if (_Claims.Organisation.Category.Name == DfeSignInRoles.CategoryTypeSchool)
            {
                if (_Claims.Organisation.LocalAuthority != null)
                {
                    laID = Convert.ToInt32(_Claims.Organisation.LocalAuthority.Code);
                }
            }

            if (_Claims.Organisation.Category.Name == DfeSignInRoles.CategoryTypeMAT)
            {
                laID = 0;
            }

            // Check if the policy is already cached in the session
            var cachedPolicy = HttpContext.Session.GetString("FreeSchoolMealsPolicy");
            if (string.IsNullOrEmpty(cachedPolicy))
            {
                // If not cached, retrieve from the Local Authority API
                var localAuthoritySettings = await _localAuthoritySettingsGateway.GetLocalAuthoritySettingsAsync(laID);
                policy = localAuthoritySettings?.EligibilityPolicies?.FirstOrDefault(p => p.CheckType == CheckEligibilityType.FreeSchoolMeals.ToString());

                // Cache the policy in the session for future requests
                HttpContext.Session.SetString("FreeSchoolMealsPolicy", JsonConvert.SerializeObject(policy));
            }
            else
            {
                // If cached, deserialize it from the session
                policy = JsonConvert.DeserializeObject<EligibilityPolicyAssignment>(cachedPolicy);
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error retrieving Free School Meals policy: {ex.Message}");
            policy = new EligibilityPolicyAssignment
            {
                CheckType = CheckEligibilityType.FreeSchoolMeals.ToString(),
                EligibilityCriteria = EligibilityCriteria.standard.ToString() // Default to standard if there's an error
            };
        }
        // Store the policy in ViewBag for easy use in views
        ViewBag.FreeSchoolMealsPolicy = policy;

        return policy;
    }

    public async Task<bool> IsExpandedFSMEnabled()
    {
        var policy = await GetFreeSchoolMealsPolicy();
        return policy != null && policy.EligibilityCriteria == EligibilityCriteria.expanded.ToString();
    }
}