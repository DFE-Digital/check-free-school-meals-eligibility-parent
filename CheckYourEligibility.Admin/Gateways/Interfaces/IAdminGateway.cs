using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IAdminGateway
{
    Task<ApplicationItemResponse> GetApplication(string id);
    Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody);
    Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status, EligibilityTier? tier = null);
    Task<ApplicationStatusRestoreResponse> RestoreApplicationStatus(string id);
    Task<int> GetMultiAcademyTrustIdForEstablishment(int establishmentId);
    Task<MultiAcademyTrustSettingsResponse?> GetMultiAcademyTrustSettingsAsync(int matId);
}