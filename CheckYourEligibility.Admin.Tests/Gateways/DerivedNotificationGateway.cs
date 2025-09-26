using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.Admin.Gateways.Tests;

internal class DerivedNotificationGateway : NotificationGateway
{
    public DerivedNotificationGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor
        )
        : base(logger, httpClient, configuration, httpContextAccessor)
    {
        apiErrorCount = 0;
    }

    public int apiErrorCount { get; private set; }

    protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri, string data)
    {
        apiErrorCount++;
        return Task.CompletedTask;
    }

    protected override Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri)
    {
        apiErrorCount++;
        return Task.CompletedTask;
    }
}