using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.UseCases;

public interface IGetCheckUseCase
{
    Task<CheckEligibilityItemResponse> Execute(string responseJson);
}

public class GetCheckUseCase : IGetCheckUseCase
{
    private readonly ICheckGateway _checkGateway;
    private readonly ILogger<GetCheckUseCase> _logger;

    public GetCheckUseCase(
        ILogger<GetCheckUseCase> logger,
        ICheckGateway checkGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public async Task<CheckEligibilityItemResponse> Execute(string responseJson)
    {
        if (string.IsNullOrEmpty(responseJson))
        {
            _logger.LogWarning("No response data found.");
            throw new Exception("No response data found.");
        }

        var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
        _logger.LogInformation($"Retrieving full check data for check.");
        var check = await _checkGateway.GetCheck(response);

        if (check?.Data == null)
        {
            _logger.LogWarning("Null response received from GetCheck.");
            throw new Exception("Null response received from GetCheck.");
        }

        _logger.LogInformation($"Retrieved full check data: {check.Data.Status}");

        return check;
    }
}