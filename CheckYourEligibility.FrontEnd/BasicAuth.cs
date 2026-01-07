using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration
    ) : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var password = _configuration["BasicPassword"];
        if (string.IsNullOrEmpty(password))
        {
            var claims = new[] { new Claim("name", "parent"), new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims, "Basic");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Basic ".Length).Trim();
            var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialString.Split(':');
            if (credentials[1] == password)
            {
                var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "Admin") };
                var identity = new ClaimsIdentity(claims, "Basic");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                return Task.FromResult(
                    AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }

            Response.StatusCode = 401;
            Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\"");
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }

        var refererHeader = Request.Headers["Referer"].ToString();
        string[] allowedReferers = _configuration["BasicReferer"].Split("|");
        if (allowedReferers.Length<1)
        {
            foreach (var allowedReferer in allowedReferers)
            {
                if (!string.IsNullOrEmpty(refererHeader) && refererHeader.Contains(allowedReferer))
                {
                    var claims = new[] { new Claim("name", allowedReferer), new Claim(ClaimTypes.Role, "Admin") };
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var claimsPrincipal = new ClaimsPrincipal(identity);

                    Context.Response.Cookies.Append("referer", allowedReferer);

                    return Task.FromResult(
                        AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }

                if (Context.Request.Cookies["referer"] != null && Context.Request.Cookies["referer"] == allowedReferer)
                {
                    var claims = new[] { new Claim("name", allowedReferer), new Claim(ClaimTypes.Role, "Admin") };
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var claimsPrincipal = new ClaimsPrincipal(identity);

                    return Task.FromResult(
                        AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }
            }
        }

        Response.StatusCode = 401;
        Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\"");
        return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }
}