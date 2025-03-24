using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IInitializeCheckAnswersUseCase
    {
        Task<FsmApplication> Execute(string applicationJson);
    }

    public class InitializeCheckAnswersUseCase : IInitializeCheckAnswersUseCase
    {
        private readonly ILogger<InitializeCheckAnswersUseCase> _logger;

        public InitializeCheckAnswersUseCase(ILogger<InitializeCheckAnswersUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<FsmApplication> Execute(string applicationJson)
        {
            try
            {
                if (string.IsNullOrEmpty(applicationJson))
                {
                    _logger.LogInformation("No FSM application data found in TempData");
                    return Task.FromResult<FsmApplication>(null);
                }

                FsmApplication application;
                try
                {
                    application = JsonConvert.DeserializeObject<FsmApplication>(applicationJson);
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize FSM application data");
                    return Task.FromResult<FsmApplication>(null);
                }

                if (application == null)
                {
                    _logger.LogWarning("Failed to deserialize FSM application data");
                    return Task.FromResult<FsmApplication>(null);
                }

                _logger.LogInformation("Successfully initialized Check_Answers view for parent: {ParentName}",
                    $"{application.ParentFirstName} {application.ParentLastName}");

                return Task.FromResult(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Check_Answers view");
                throw;
            }
        }
    }
}