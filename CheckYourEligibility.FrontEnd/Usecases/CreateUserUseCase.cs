using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface ICreateUserUseCase
{
    Task<string> Execute(string email, string uniqueId);
}

public class CreateUserUseCase : ICreateUserUseCase
{
    private readonly IParentGateway _parentGatewayService;

    public CreateUserUseCase(IParentGateway parentGatewayService)
    {
        _parentGatewayService = parentGatewayService ?? throw new ArgumentNullException(nameof(parentGatewayService));
    }

    public async Task<string> Execute(string email, string uniqueId)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrEmpty(uniqueId))
            throw new ArgumentException("Unique ID cannot be empty", nameof(uniqueId));

        var userRequest = new UserCreateRequest
        {
            Data = new UserData
            {
                Email = email,
                Reference = uniqueId
            },
            MetaData = new CheckMetaData
            {
                UserName = email,
                OrganisationID = 0,
                OrganisationType = "none"
            }
        };

        var response = await _parentGatewayService.CreateUser(userRequest);
        return response?.Data ?? throw new Exception("Failed to create user");
    }
}