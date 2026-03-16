using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Gateways;

public sealed class LocalAuthoritySettingsGateway : BaseGateway, ILocalAuthoritySettingsGateway
{


    public LocalAuthoritySettingsGateway(
        ILoggerFactory logger,
        HttpClient httpClient,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base("EcsService", logger, httpClient, configuration, httpContextAccessor)
    {
    }    

    public async Task<LocalAuthoritySettingsResponse?> GetLocalAuthoritySettingsAsync(int laCode)
    {
        var url = $"local-authorities/{laCode}/settings";

        return await ApiDataGetAsynch(url, new LocalAuthoritySettingsResponse());
    }
}