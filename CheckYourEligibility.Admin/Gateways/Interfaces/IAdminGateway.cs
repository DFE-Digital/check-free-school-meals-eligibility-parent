using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Domain;

namespace CheckYourEligibility.Admin.Gateways.Interfaces
{
    public interface IAdminGateway
    {
        Task<ApplicationItemResponse> GetApplication(string id);
        Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch2 requestBody);
        Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status);
    }
}