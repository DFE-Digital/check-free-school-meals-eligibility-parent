using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Gateways;

public class ParentGateway : BaseGateway, IParentGateway
{
    private readonly string _ApplicationUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _schoolUrl;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    public ParentGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(
        "EcsService", logger, httpClient, configuration, httpContextAccessor)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
        _ApplicationUrl = "application";
        _schoolUrl = "establishment";
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<EstablishmentSearchResponse> GetSchool(string name, string la)
    {
        try
        {
            var response = await ApiDataGetAsynch($"{_httpClient.BaseAddress}{_schoolUrl}/search?query={name}&la={la}",
                new EstablishmentSearchResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Get School failed. uri-{_httpClient.BaseAddress}{_schoolUrl}/search?query={name}&la={la}");
            throw;
        }
    }

    public async Task<ApplicationSaveItemResponse> PostApplication_Fsm(ApplicationRequest requestBody)
    {
        try
        {
            requestBody.Data.Type = CheckEligibilityType.FreeSchoolMeals;
            var response =
                await ApiDataPostAsynch($"{_ApplicationUrl}", requestBody, new ApplicationSaveItemResponse());
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