using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.Admin.Gateways.Tests
{
    internal class DerivedCheckGateway : CheckGateway
    {

        public int apiErrorCount { get; private set; }

        public DerivedCheckGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
            : base(logger, httpClient, configuration)
        {
            apiErrorCount = 0;
        }

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
}
