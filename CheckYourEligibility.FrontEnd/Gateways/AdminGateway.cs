using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Domain.Enums;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using Newtonsoft.Json;

namespace CheckYourEligibility.FrontEnd.Gateways;

public class AdminGateway : BaseGateway, IAdminGateway
{
    private readonly string _ApplicationSearchUrl = "application/search";
    private readonly string _ApplicationUrl = "/application";
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public AdminGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
    }


    public async Task<ApplicationItemResponse> GetApplication(string id)
    {
        try
        {
            var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress}{_ApplicationUrl}/{id}",
                new ApplicationItemResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Get failed. uri-{_httpClient.BaseAddress}{_ApplicationUrl}/{id}");
            throw;
        }
    }

    public async Task<ApplicationSearchResponse> PostApplicationSearch(ApplicationRequestSearch requestBody)
    {
        try
        {
            var result = await ApiDataPostAsynch(_ApplicationSearchUrl, requestBody, new ApplicationSearchResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
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
            var result = await ApiDataPatchAsynch(url, request, new ApplicationStatusUpdateResponse());
            if (result.Data.Status != status.ToString()) throw new Exception("Failed to update status");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post failed. uri:-{_httpClient.BaseAddress}{_ApplicationSearchUrl} content:-{JsonConvert.SerializeObject(request)}");
            throw;
        }
    }
}