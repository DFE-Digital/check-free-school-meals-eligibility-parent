using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface ILoadParentDetailsUseCase
    {
        Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null
        );
    }

    public class LoadParentDetailsUseCase : ILoadParentDetailsUseCase
    {
        private readonly ILogger<LoadParentDetailsUseCase> _logger;

        public LoadParentDetailsUseCase(ILogger<LoadParentDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(ParentGuardian Parent, Dictionary<string, List<string>> ValidationErrors)> Execute(
            string parentDetailsJson = null,
            string validationErrorsJson = null)
        {
            ParentGuardian parent = null;
            Dictionary<string, List<string>> errors = null;

            
            if (!string.IsNullOrEmpty(parentDetailsJson))
            {
                try
                {
                    parent = JsonConvert.DeserializeObject<ParentGuardian>(parentDetailsJson);
                }
                
                catch (JsonException)
                {
                }
            }

            
            if (!string.IsNullOrEmpty(validationErrorsJson))
            {
                errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(validationErrorsJson);
                if (errors != null)
                {
                    const string targetValue = "Please select one option";

                    if (errors.ContainsKey("NationalInsuranceNumber") &&
                        errors.ContainsKey("NationalAsylumSeekerServiceNumber"))
                    {
                        if (errors["NationalInsuranceNumber"].Contains(targetValue) &&
                            errors["NationalAsylumSeekerServiceNumber"].Contains(targetValue))
                        {
                            errors.Remove("NationalInsuranceNumber");
                            errors.Remove("NationalAsylumSeekerServiceNumber");
                            errors["NINAS"] = new List<string> { targetValue };
                        }
                    }
                }
            }

            return (parent, errors);
        }
    }
}