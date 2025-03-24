using CheckYourEligibility.Admin.Domain.DfeSignIn;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

namespace CheckYourEligibility.Admin.Infrastructure;
public static class DfeSignInExtensions
{
    /// <summary>
    /// Add support for DfE sign-in authentication using Open ID.
    /// </summary>
    /// <param name="configuration">Configuration options.</param>
    /// <seealso cref="AddDfeSignInPublicApi"/>
    public static void AddDfeSignInAuthentication(this IServiceCollection services, IDfeSignInConfiguration configuration)
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
                foreach (string scope in configuration.Scopes)
                {
                    options.Scope.Add(scope);
                }

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
    }

    public static DfeClaims? GetDfeClaims(IEnumerable<Claim> claims)
    {
        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims));
        }
        var result = new DfeClaims() {
            Organisation = GetOrganisation(claims),
            User = GetUser(claims),
        };

        return result;
    }

    private static Organisation? GetOrganisation(IEnumerable<Claim> claims)
    {
        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims));
        }

        var organisationJson = claims.Where(c => c.Type == ClaimConstants.Organisation)
            .Select(c => c.Value)
        .FirstOrDefault();

        if (organisationJson == null)
        {
            return null;
        }

        var organisation = JsonHelpers.Deserialize<Organisation>(organisationJson)!;

        if (organisation.Id == Guid.Empty)
        {
            return null;
        }

        return organisation;
    }
    private static UserInformation GetUser(IEnumerable<Claim> claims)
    {
        var userInformation= new UserInformation();

        userInformation.Id = claims.Where(c => c.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}")
            .Select(c => c.Value).First();
        userInformation.Email = claims.Where(c => c.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
           .Select(c => c.Value).First();
        userInformation.FirstName = claims.Where(c => c.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
           .Select(c => c.Value).First();
        userInformation.Surname = claims.Where(c => c.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
           .Select(c => c.Value).First();

        return userInformation;
    }
}
