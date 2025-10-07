using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IParentGateway
{
    Task<EstablishmentSearchResponse> GetSchool(string name, string organisationNumber, string organisationType);

    Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody);

    Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody);
}