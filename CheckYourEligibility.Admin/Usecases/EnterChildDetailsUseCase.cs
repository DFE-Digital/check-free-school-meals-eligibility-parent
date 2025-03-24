using CheckYourEligibility.Admin.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IEnterChildDetailsUseCase
    {
       
        Children Execute(
            string childListJson = null,
            bool? isChildAddOrRemove = null);
    }

    public class EnterChildDetailsUseCase : IEnterChildDetailsUseCase
    {
        private readonly ILogger<EnterChildDetailsUseCase> _logger;

        public EnterChildDetailsUseCase(ILogger<EnterChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Children Execute(
            string childListJson = null,
            bool? isChildAddOrRemove = null)
        {
            // Initialize default model with one empty child
            var children = new Children { ChildList = new List<Child> { new Child() } };

            // Handle redirect after add/remove child operations
            if (isChildAddOrRemove == true && !string.IsNullOrEmpty(childListJson))
            {
                var deserializedChildList = JsonConvert.DeserializeObject<List<Child>>(childListJson);
                if (deserializedChildList != null)
                {
                    children.ChildList = deserializedChildList;
                }
            }

            return children;
        }
    }
}