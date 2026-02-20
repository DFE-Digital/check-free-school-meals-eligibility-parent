using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Gateways;

public class CheckGateway : BaseGateway, ICheckGateway
{
    private readonly string _FsmCheckBulkUploadUrl;
    private readonly string _FsmCheckUrl;
    private readonly HttpClient _httpClient;
    private readonly string _EligibilityCheckReportUrl;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public CheckGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base("EcsService",
        logger, httpClient, configuration, httpContextAccessor)
    {
        _logger = logger.CreateLogger("EcsService");
        _httpClient = httpClient;
        _FsmCheckUrl = "check/free-school-meals";
        _FsmCheckBulkUploadUrl = "bulk-check/free-school-meals";
        _httpContextAccessor = httpContextAccessor;
        _EligibilityCheckReportUrl = "eligibility-check/report";

    }

    public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Fsm requestBody)
    {
        try
        {
            var result = await ApiDataPostAsynch(_FsmCheckUrl, requestBody, new CheckEligibilityResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post Check failed. uri:-{_httpClient.BaseAddress}{_FsmCheckUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }

    public async Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody)
    {
        try
        {
            var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}/status",
                new CheckEligibilityStatusResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}/status");
        }

        return null;
    }

    public async Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(bulkCheckUrl, new CheckEligibilityBulkStatusResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl}");
        }

        return null;
    }

    public async Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(resultsUrl, new CheckEligibilityBulkResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl}");
            throw;
        }
    }


    public async Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk_Fsm requestBody)
    {
        try
        {
            var result =
                await ApiDataPostAsynch(_FsmCheckBulkUploadUrl, requestBody, new CheckEligibilityResponseBulk());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }

    // FSM Basic Bulk Check Methods
    public async Task<CheckEligibilityResponseBulk> PostBulkCheck_FsmBasic(CheckEligibilityRequestBulk_FsmBasic requestBody)
    {
        try
        {
            var result =
                await ApiDataPostAsynch(_FsmCheckBulkUploadUrl, requestBody, new CheckEligibilityResponseBulk());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Post failed. uri:-{_httpClient.BaseAddress}{_FsmCheckBulkUploadUrl} content:-{JsonConvert.SerializeObject(requestBody)}");
            throw;
        }
    }

    public async Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress_FsmBasic(string bulkCheckUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(bulkCheckUrl, new CheckEligibilityBulkStatusResponse());
            return result;
        }
        catch (Exception ex)
        {
            var safeUrl = bulkCheckUrl?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{safeUrl}");
        }

        return null;
    }

    public async Task<CheckEligibilityBulkResponse> GetBulkCheckResults_FsmBasic(string resultsUrl)
    {
        try
        {
            var result = await ApiDataGetAsynch(resultsUrl, new CheckEligibilityBulkResponse());
            return result;
        }
        catch (Exception ex)
        {
            var safeUrl = resultsUrl?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}{safeUrl}");
            throw;
        }
    }

    public async Task<CheckEligibilityBulkProgressByLAResponse> GetBulkCheckStatuses_FsmBasic(string organisationId)
    {
        try
        {
            var url = $"bulk-check/search?organisationId={organisationId}";
            var result = await ApiDataGetAsynch(url, new CheckEligibilityBulkProgressByLAResponse());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}bulk-check/search");
            throw;
        }
    }

    public async Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecksFor_FsmBasic(string bulkCheckDeleteUrl)
    {
        try
        {
            var result = await ApiDataDeleteAsynch(bulkCheckDeleteUrl, new CheckEligiblityBulkDeleteResponse());
            return result;
        }
        catch (Exception ex)
        {
            var safeUrl = bulkCheckDeleteUrl?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, $"delete failed. uri:-{_httpClient.BaseAddress}{safeUrl}");
            throw;
        }
    }

    public async Task<IEnumerable<IBulkExport>> LoadBulkCheckResults_FsmBasic(string bulkCheckId)
    {
        try
        {
            var url = $"bulk-check/{bulkCheckId}/";
            var response = await GetBulkCheckResults_FsmBasic(url);
            
            if (response?.Data == null)
            {
                return Enumerable.Empty<IBulkExport>();
            }

            return response.Data.Select(x => new BulkExport
            {
                LastName = x.LastName,
                DOB = x.DateOfBirth,
                NI = x.NationalInsuranceNumber,
                Outcome = GetFsmBasicStatusDescription(x.Status),
            });
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, $"LoadBulkCheckResults_FsmBasic failed for bulkCheckId: {safeBulkCheckId}");
            throw;
        }
    }

    private string GetFsmBasicStatusDescription(string status)
    {
        if (string.IsNullOrEmpty(status))
            return status;

        return status switch
        {
            "parentNotFound" => "Information does not match records",
            "eligible" => "Entitled",
            "notEligible" => "Not Entitled",
            "error" => "Try again",
            _ => status
        };
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

}