﻿using CheckYourEligibility.FrontEnd.Gateways;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.FrontEnd.Services.Tests.Parent;

public class EcsServiceParentTest : ParentGateway
{
    public EcsServiceParentTest(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
        : base(logger, httpClient, configuration)
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