using CheckYourEligibility.Admin.Models;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IAddChildUseCase
    {
        Children Execute(Children request);
    }
    
    [Serializable]
    public class MaxChildrenException : Exception
    {
        
        public MaxChildrenException(string message) : base (message)
        {
        }
    }

    public class AddChildUseCase : IAddChildUseCase
    {
        private readonly ILogger<AddChildUseCase> _logger;
        private readonly IConfiguration _configuration;
        
        public AddChildUseCase(ILogger<AddChildUseCase> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
        }

        public Children Execute(Children request)
        {
            if (request == null||request.ChildList == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.ChildList.Count >= _configuration.GetValue<int>("MaxChildren"))
            {
                throw new MaxChildrenException("");
            }

            request.ChildList.Add(new Child());

            _logger.LogInformation("Successfully added new child. Total children: {Count}", request.ChildList.Count);
            return request;
        }
    }
}