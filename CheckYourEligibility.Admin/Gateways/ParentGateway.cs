using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CheckYourEligibility.Admin.Gateways
{
    public class ParentGateway : BaseGateway,  IParentGateway
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationUrl;
        private readonly string _schoolUrl;

        public  ParentGateway(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;
            _ApplicationUrl = "application";
            _schoolUrl = "establishment";
        }

        public async Task<EstablishmentSearchResponse> GetSchool(string name)
        {
            try
            {
                var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress}{_schoolUrl}/search?query={name}", new EstablishmentSearchResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get School failed. uri-{_httpClient.BaseAddress}{_schoolUrl}/search?query={name}");
                throw;
            }
        }

        public async Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody)
        {
            try
            {
                requestBody.Data.Type = CheckYourEligibility.Domain.Enums.CheckEligibilityType.FreeSchoolMeals;
                var response = await ApiDataPostAsynch($"{_ApplicationUrl}", requestBody, new ApplicationSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post Application failed. uri-{_httpClient.BaseAddress}{_ApplicationUrl}");
                throw;
            }
        }

        public async Task<UserSaveItemResponse> CreateUser(UserCreateRequest requestBody)
        {
            try
            {
                var response = await ApiDataPostAsynch("user", requestBody, new UserSaveItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Create User failed. uri-{_httpClient.BaseAddress}user");
            }

            return null;
        }
    }
}
