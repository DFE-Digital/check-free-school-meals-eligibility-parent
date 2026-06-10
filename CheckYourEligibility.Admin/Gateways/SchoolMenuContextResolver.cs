using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CheckYourEligibility.Admin.Gateways;

public class SchoolMenuContextResolver : ISchoolMenuContextResolver
{
    private readonly IMemoryCache _cache;
    private readonly IAdminGateway _adminGateway;
    private readonly ILocalAuthoritySettingsGateway _localAuthoritySettingsGateway;
    private readonly ILogger<SchoolMenuContextResolver> _logger;

    public SchoolMenuContextResolver(
        IMemoryCache cache,
        IAdminGateway adminGateway,
        ILocalAuthoritySettingsGateway localAuthoritySettingsGateway,
        ILogger<SchoolMenuContextResolver> logger)
    {
        _cache = cache;
        _adminGateway = adminGateway;
        _localAuthoritySettingsGateway = localAuthoritySettingsGateway;
        _logger = logger;
    }

    public async Task<SchoolMenuContext> ResolveAsync(DfeClaims claims)
    {
        var context = new SchoolMenuContext();

        var isSchoolUser = claims?.Roles?.Any(r =>
            string.Equals(r.Code, DfeSignInRoles.RoleCodeSchool, StringComparison.OrdinalIgnoreCase)) == true;

        if (!isSchoolUser)
        {
            _logger.LogInformation("SchoolMenuContextResolver: user is not a school user");
            return context;
        }

        context.IsSchool = true;

        var establishmentIdString = claims?.Organisation?.Urn;
        if (!int.TryParse(establishmentIdString, out var establishmentId))
        {
            _logger.LogInformation(
                "SchoolMenuContextResolver: invalid establishment id Est={EstablishmentId}",
                establishmentIdString);
            return context;
        }

        var laCodeString = claims?.Organisation?.LocalAuthority?.Code;
        if (int.TryParse(laCodeString, out var laCode))
        {
            context.LaCode = laCode;
        }

        var matIdCacheKey = $"SchoolMatId_{establishmentId}";
        int matId;

        if (!_cache.TryGetValue(matIdCacheKey, out matId))
        {
            matId = await _adminGateway.GetMultiAcademyTrustIdForEstablishment(establishmentId);
            _cache.Set(matIdCacheKey, matId, TimeSpan.FromMinutes(5));
        }

        context.MatId = matId > 0 ? matId : null;
        context.IsPartOfMat = matId > 0;

        if (context.IsPartOfMat)
        {
            var matCacheKey = $"MatSettings_{matId}";
            MultiAcademyTrustSettingsResponse? matSettings;

            if (!_cache.TryGetValue(matCacheKey, out matSettings))
            {
                matSettings = await _adminGateway.GetMultiAcademyTrustSettingsAsync(matId);

                if (matSettings != null)
                {
                    _cache.Set(matCacheKey, matSettings, TimeSpan.FromMinutes(5));
                }
            }

            context.ShowReviewEvidenceTiles = matSettings?.AcademyCanReviewEvidence ?? false;

            _logger.LogInformation(
                "SchoolMenuContextResolver: resolved MAT school Est={EstablishmentId} MatId={MatId} ShowTiles={ShowTiles}",
                establishmentId,
                matId,
                context.ShowReviewEvidenceTiles);

            return context;
        }

        if (!context.LaCode.HasValue)
        {
            _logger.LogInformation(
                "SchoolMenuContextResolver: no LA code for non-MAT school Est={EstablishmentId}",
                establishmentId);
            return context;
        }

        var laCacheKey = $"LocalAuthoritySettings_{context.LaCode.Value}";
        LocalAuthoritySettingsResponse? localAuthoritySettings;

        if (!_cache.TryGetValue(laCacheKey, out localAuthoritySettings))
        {
            localAuthoritySettings =
                await _localAuthoritySettingsGateway.GetLocalAuthoritySettingsAsync(context.LaCode.Value);

            if (localAuthoritySettings != null)
            {
                _cache.Set(laCacheKey, localAuthoritySettings, TimeSpan.FromMinutes(5));
            }
        }

        context.ShowReviewEvidenceTiles = localAuthoritySettings?.SchoolCanReviewEvidence ?? false;

        _logger.LogInformation(
            "SchoolMenuContextResolver: resolved LA school Est={EstablishmentId} LaCode={LaCode} ShowTiles={ShowTiles}",
            establishmentId,
            context.LaCode.Value,
            context.ShowReviewEvidenceTiles);

        return context;
    }
}