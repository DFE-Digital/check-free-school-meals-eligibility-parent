using System.Security.Claims;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.DfeSignIn.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CheckYourEligibility.Admin.Infrastructure;

public static class DfeSignInExtensions
{
    /// <summary>
    ///     Add support for DfE sign-in authentication using Open ID.
    /// </summary>
    /// <param name="configuration">Configuration options.</param>
    /// <seealso cref="AddDfeSignInPublicApi" />
    public static void AddDfeSignInAuthentication(this IServiceCollection services,
        IDfeSignInConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(options =>
            {
                options.ClientId = configuration.ClientId;
                options.ClientSecret = configuration.ClientSecret;

                options.Authority = configuration.Authority;
                options.MetadataAddress = configuration.MetaDataUrl;
                options.CallbackPath = new PathString(configuration.CallbackUrl);
                options.SignedOutRedirectUri = new PathString(configuration.SignoutRedirectUrl);
                options.SignedOutCallbackPath = new PathString(configuration.SignoutCallbackUrl);
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.Scope.Clear();
                foreach (var scope in configuration.Scopes) options.Scope.Add(scope);

                options.GetClaimsFromUserInfoEndpoint = configuration.GetClaimsFromUserInfoEndpoint;
                options.SaveTokens = configuration.SaveTokens;
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();

                        return Task.FromResult(0);
                    }
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = configuration.CookieName;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.CookieExpireTimeSpanInMinutes);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = configuration.SlidingExpiration;
            });

        // Register the DfE Sign-in API service for fetching user roles
        services.AddHttpClient<IDfeSignInApiService, DfeSignInApiService>();
    }

    public static DfeClaims? GetDfeClaims(IEnumerable<Claim> claims)
    {
        if (claims == null || !claims.Any()) return null;
        var result = new DfeClaims
        {
            Organisation = GetOrganisation(claims),
            User = GetUser(claims),
            Roles = GetRoles(claims)
        };

        return result;
    }

    private static Organisation? GetOrganisation(IEnumerable<Claim> claims)
    {
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        var organisationJson = claims.Where(c => c.Type == ClaimConstants.Organisation)
            .Select(c => c.Value)
            .FirstOrDefault();

        if (organisationJson == null) return null;

        var organisation = JsonHelpers.Deserialize<Organisation>(organisationJson)!;

        if (organisation.Id == Guid.Empty) return null;

        return organisation;
    }

    private static UserInformation GetUser(IEnumerable<Claim> claims)
    {
        var userInformation = new UserInformation();

        userInformation.Id = claims.Where(c =>
                c.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}")
            .Select(c => c.Value).First();
        userInformation.Email = claims
            .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
            .Select(c => c.Value).First();
        userInformation.FirstName = claims
            .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
            .Select(c => c.Value).First();
        userInformation.Surname = claims
            .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
            .Select(c => c.Value).First();

        return userInformation;
    }

    private static IList<Role> GetRoles(IEnumerable<Claim> claims)
    {
        var roles = new List<Role>();
        if (claims == null) return roles;

        // Try to get roles from a JSON claim (if roles come as a JSON array)
        var rolesJson = claims.Where(c => c.Type == ClaimConstants.Role || c.Type == "roles")
            .Select(c => c.Value)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(rolesJson) && rolesJson.TrimStart().StartsWith("["))
        {
            try
            {
                var parsedRoles = JsonHelpers.Deserialize<List<Role>>(rolesJson);
                if (parsedRoles != null)
                {
                    return parsedRoles;
                }
            }
            catch
            {
                // If JSON parsing fails, continue to try individual claims
            }
        }

        // Try to get role from individual claims
        var roleId = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleId)?.Value;
        var roleName = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleName)?.Value;
        var roleCode = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleCode)?.Value;
        var roleNumericId = claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleNumericId)?.Value;

        if (!string.IsNullOrEmpty(roleCode) || !string.IsNullOrEmpty(roleName))
        {
            var role = new Role
            {
                Code = roleCode ?? string.Empty,
                Name = roleName ?? string.Empty,
                NumericId = roleNumericId ?? string.Empty
            };

            if (Guid.TryParse(roleId, out var parsedRoleId))
            {
                role.Id = parsedRoleId;
            }

            roles.Add(role);
        }

        return roles;
    }
}