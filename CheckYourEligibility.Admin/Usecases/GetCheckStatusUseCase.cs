using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IGetCheckStatusUseCase
    {
        Task<string> Execute(string responseJson, ISession session);
    }

    public class GetCheckStatusUseCase : IGetCheckStatusUseCase
    {
        private readonly ILogger<GetCheckStatusUseCase> _logger;
        private readonly ICheckGateway _checkGateway;

        public GetCheckStatusUseCase(
            ILogger<GetCheckStatusUseCase> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
        }

        public async Task<string> Execute(string responseJson, ISession session)
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogWarning("No response data found in TempData.");
                throw new Exception("No response data found in TempData.");
            }

            var response = JsonConvert.DeserializeObject<CheckEligibilityResponse>(responseJson);
            _logger.LogInformation($"Check status processed: {response.Data.Status}");
            var check = await _checkGateway.GetStatus(response);

            if (check?.Data == null)
            {
                _logger.LogWarning("Null response received from GetStatus.");
                throw new Exception("Null response received from GetStatus.");
            }

            _logger.LogInformation($"Received status: {check.Data.Status}");
            session.SetString("CheckResult", check.Data.Status);

            return check.Data.Status;
        }
    }
}
