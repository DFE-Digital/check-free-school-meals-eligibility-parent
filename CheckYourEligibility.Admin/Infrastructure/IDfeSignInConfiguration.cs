namespace CheckYourEligibility.Admin.Infrastructure;

/// <summary>
/// Configuration for the DfE Sign-in service.
/// </summary>
/// <seealso cref="DfeSignInExtensions.AddDfeSignInAuthentication"/>
public interface IDfeSignInConfiguration
{
    /// <see cref="Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.Authority">
    string Authority { get; }

    /// <see cref="Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.MetadataAddress">
    string MetaDataUrl { get; }

    /// <summary>
    /// Gets the client ID of the service.
    /// </summary>
    /// <remarks>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string ClientId { get; }

    /// <summary>
    /// Gets the client secret which is required for interacting with DfE sign-in.
    /// </summary>
    /// <remarks>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string ClientSecret { get; }

    /// <summary>
    /// Gets the absolute URL Path. Set ONLY if Required By Middleware
    /// </summary>
    string APIServiceProxyUrl { get; }

    /// <summary>
    /// Gets the callback URL which the DfE Sign-in service will invoke to continue the
    /// sign-in user flow (eg. "/auth/cb").
    /// </summary>
    /// <remarks>
    /// <para>The .NET library deals with setting up this endpoint automatically; this
    /// property just indicates what the endpoint will be.</para>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string CallbackUrl { get; }

    /// <summary>
    /// Gets the name of the cookie to maintain user sign-in session.
    /// </summary>
    string CookieName { get; }

    /// <summary>
    /// Gets the cookie expiration time span in minutes.
    /// </summary>
    int CookieExpireTimeSpanInMinutes { get; }

    /// <see cref="Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions.SlidingExpiration">
    bool SlidingExpiration { get; }

    /// <see cref="Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions.AccessDeniedPath">
    string AccessDeniedPath { get; }

    /// <see cref="Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.GetClaimsFromUserInfoEndpoint">
    bool GetClaimsFromUserInfoEndpoint { get; }

    /// <see cref="Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions.SaveTokens">
    bool SaveTokens { get; }

    /// <summary>
    /// Gets the list of scopes which should be included in the JWT that is returned by
    /// DfE Sign-in.
    /// </summary>
    IList<string> Scopes { get; }

    /// <summary>
    /// Gets the callback URL which the DfE Sign-in service will invoke to continue the
    /// sign-out user flow (eg. "/signout/complete").
    /// </summary>
    /// <remarks>
    /// <para>The .NET library deals with setting up this endpoint automatically; this
    /// property just indicates what the endpoint will be.</para>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string SignoutCallbackUrl { get; }

    /// <summary>
    /// Gets the URL that the website should redirect to once the user has signed out.
    /// </summary>
    /// <remarks>
    /// <para>This should reflect the configuration in the DfE Sign-in "Manage service"
    /// web interface.</para>
    /// </remarks>
    string SignoutRedirectUrl { get; }

   
}
