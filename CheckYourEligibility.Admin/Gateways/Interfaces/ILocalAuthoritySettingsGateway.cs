using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface ILocalAuthoritySettingsGateway
{
    Task<LocalAuthoritySettingsResponse?> GetLocalAuthoritySettingsAsync(int laCode);
}