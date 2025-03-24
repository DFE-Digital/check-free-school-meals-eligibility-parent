using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces
{
    public interface IParentGateway
    {
        Task<EstablishmentSearchResponse> GetSchool(string name);
       
        Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);

        Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody);
    }
}