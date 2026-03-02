namespace CheckYourEligibility.Admin
{
    public interface ILocalAuthoritySettingsClient
    {
        Task<bool> GetSchoolCanReviewEvidenceAsync(int laCode, CancellationToken ct = default);
    }

}
