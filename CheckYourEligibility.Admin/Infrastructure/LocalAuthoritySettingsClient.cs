using System.Net;
using System.Net.Http.Json;

namespace CheckYourEligibility.Admin.EligibilityCheckingEngine;

public interface ILocalAuthoritySettingsClient
{
    Task<bool> GetSchoolCanReviewEvidenceAsync(int laCode, CancellationToken ct = default);
}

internal sealed class LocalAuthoritySettingsClient(HttpClient http) : ILocalAuthoritySettingsClient
{
    private sealed class LocalAuthoritySettingsResponse
    {
        public bool SchoolCanReviewEvidence { get; set; }
    }

    public async Task<bool> GetSchoolCanReviewEvidenceAsync(int laCode, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/local-authorities/{laCode}/settings", ct);

        if (resp.StatusCode == HttpStatusCode.Forbidden)
        {
            // “affected LA only” restriction -> treat as false for safety
            return false;
        }

        if (!resp.IsSuccessStatusCode) return false;

        var body = await resp.Content.ReadFromJsonAsync<LocalAuthoritySettingsResponse>(cancellationToken: ct);
        return body?.SchoolCanReviewEvidence ?? false;
    }
}