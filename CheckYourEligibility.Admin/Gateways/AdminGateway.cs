using CheckYourEligibility.Admin.Domain;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Admin.Domain;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Gateways
{
    public class AdminGateway : BaseGateway, IAdminGateway
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _ApplicationSearchUrl = "application/search";
        private readonly string _ApplicationUrl = "/application";

        public AdminGateway(ILoggerFactory logger, HttpClient httpClient,IConfiguration configuration): base("EcsService", logger, httpClient, configuration)
        {
            _logger = logger.CreateLogger("EcsService");
            _httpClient = httpClient;       
        }

       
        public async Task<ApplicationItemResponse> GetApplication(string id)
        {
            try
            {
                // TODO: Check the first slash part of the _ApplicationUrl
                var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress?.OriginalString}{_ApplicationUrl}/{id}", new ApplicationItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get failed. uri-{_httpClient.BaseAddress}{_ApplicationUrl}/{id}");
                throw;
            }
        }

        public async Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch2 requestBody)
        {
            try
            {
                var result = await ApiDataPostAsynch(_ApplicationSearchUrl, requestBody, new ApplicationSearchResponse());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
                throw;
            }
        }

        public async Task<ApplicationStatusUpdateResponse> PatchApplicationStatus(string id, ApplicationStatus status)
        {
            var url = $"{_ApplicationUrl}/{id}";
            var request = new ApplicationStatusUpdateRequest
            {
                Data = new ApplicationStatusData { Status = status }
            };
            try
            {    
                var result = await ApiDataPatchAsynch(url,request,new ApplicationStatusUpdateResponse());
                if (result.Data.Status != status.ToString()) {
                    throw new Exception("Failed to update status");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Post failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(request)}");
                throw;
            }
        }
    }
}
