namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface ILocalAuthoritySettingsGateway
{
    Task<bool> GetSchoolCanReviewEvidenceAsync(int laCode);
}