using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;
using Newtonsoft.Json;
using System.Net;

namespace CheckYourEligibility.Admin.Gateways;

public class EligibilityCheckReportingGateway : BaseGateway, IEligibilityCheckReportingGateway
{
    private readonly HttpClient _httpClient;
    private readonly string _EligibilityCheckReportUrl;
    private readonly string _EligibilityCheckReportHistory;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public EligibilityCheckReportingGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _EligibilityCheckReportUrl = "/check-eligibility/report";
        _EligibilityCheckReportHistory = "/check-eligibility/report-history/";

    }
    public async Task<EligibilityCheckReportResponse> GenerateEligibilityCheckReport(
    EligibilityCheckReportRequest requestBody)
    {
        try
        {
            var result = await ApiDataPostAsynch(
                _EligibilityCheckReportUrl,
                requestBody,
                new EligibilityCheckReportResponse()
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"GenerateEligibilityCheckReport failed. uri:-{_httpClient.BaseAddress}{_EligibilityCheckReportUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }
    public async Task<EligibilityCheckReportHistoryResponse> GetEligibilityCheckReportHistory(string localAuthorityId, int pageNumber)
    {
        try
        {
            var url = $"{_EligibilityCheckReportHistory}{localAuthorityId}?pageNumber={pageNumber}";
            var result = await ApiDataGetAsynch(
                url,
                new EligibilityCheckReportHistoryResponse()
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"GetEligibilityCheckReportHistory failed. uri:-{_httpClient.BaseAddress}{_EligibilityCheckReportHistory}{localAuthorityId}");
            throw;
        }
    }

    public async Task DeleteEligibilityCheckReport(Guid reportId)
    {
        try
        {
            var url = $"{_EligibilityCheckReportUrl}{reportId}";

            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                await LogApiError(response, "DELETE", url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();
            }
        }
        catch (Exception ex)
        {
          
        }
    }

}
