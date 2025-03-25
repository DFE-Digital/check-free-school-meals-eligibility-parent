using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain;
using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IAdminGateway
{
    Task<ApplicationItemResponse> GetApplication(string id);
    Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch2 requestBody);
    Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
}