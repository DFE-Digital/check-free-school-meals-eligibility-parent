using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Gateways;

public sealed class LocalAuthoritySettingsGateway : BaseGateway, ILocalAuthoritySettingsGateway
{
    private readonly HttpClient _httpClient;

    public LocalAuthoritySettingsGateway(
        ILoggerFactory logger,
        HttpClient httpClient,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base("EcsService", logger, httpClient, configuration, httpContextAccessor)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> GetSchoolCanReviewEvidenceAsync(int laCode)
    {
        var url = $"{_httpClient.BaseAddress}local-authorities/{laCode}/settings";

        var response = await ApiDataGetAsynch(url, new LocalAuthoritySettingsResponse());
        return response?.SchoolCanReviewEvidence ?? false;
    }
}