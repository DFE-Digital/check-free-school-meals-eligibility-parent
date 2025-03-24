using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IRegistrationResponseUseCase
    {
        Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request);
    }

    public class RegistrationResponseUseCase : IRegistrationResponseUseCase
    {
        private readonly ILogger<RegistrationResponseUseCase> _logger;

        public RegistrationResponseUseCase(ILogger<RegistrationResponseUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ApplicationConfirmationEntitledViewModel> Execute(FsmApplication request)
        {
            var parentName = $"{request.ParentFirstName} {request.ParentLastName}";
            var response = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = parentName,
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };

            foreach (var child in request.Children.ChildList)
            {
                response.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                {
                    ParentName = parentName,
                    ChildName = $"{child.FirstName} {child.LastName}",
                    Reference = $"{DateTime.Now:yyyyMMddHHmmss}-{child.ChildIndex}"
                });
            }

            _logger.LogInformation("Created registration response for parent {ParentName}", parentName);

            return Task.FromResult(response);
        }
    }
}