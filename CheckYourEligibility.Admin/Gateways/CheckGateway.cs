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
    private readonly string _EligibilityCheckReportHistory;
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
        _EligibilityCheckReportUrl = "/check-eligibility/report";
        _EligibilityCheckReportHistory = "/check-eligibility/report-history/";

    }

    public async Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Enhanced requestBody)
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
            var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheckStatus}",
                new CheckEligibilityStatusResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Status failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheckStatus}");
        }

        return null;
    }
    public async Task<EstablishmentResponse> GetAcademiesAsync(int multiAcademyTrustId)
    {
        string url = $"/multi-academy-trusts/{multiAcademyTrustId}/establishments";

        try
        {
            var response = await ApiDataGetAsynch(url, new EstablishmentResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Gettin academies for MAT failed. uri:-{_httpClient.BaseAddress}{url}");
        }

        return null;
    }
    public async Task<EstablishmentResponse> GetSchoolsAsync(int localAuthorityId)
    {       
        string url = $"/local-authorities/{localAuthorityId}/establishments"; 
        try
        {
            var response = await ApiDataGetAsynch(url, new EstablishmentResponse());      
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Gettin schools for local authority failed. uri:-{_httpClient.BaseAddress}{url}");
        }

        return null;
    }
    public async Task<CheckEligibilityItemResponse> GetCheck(CheckEligibilityResponse responseBody)
    {
        try
        {
            var response = await ApiDataGetAsynch($"{responseBody.Links.Get_EligibilityCheck}",
                new CheckEligibilityItemResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Get Check failed. uri:-{_httpClient.BaseAddress}{responseBody.Links.Get_EligibilityCheck}");
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


    public async Task<CheckEligibilityResponseBulk> PostBulkCheck<TBulk>( TBulk requestBody) where TBulk: CheckEligibilityRequestBulkBase
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

    public async Task<CheckEligibilityBulkProgressByResponseItems> GetBulkChecks()
    {
        try
        {
            var result = await ApiDataGetAsynch("/bulk-check", new CheckEligibilityBulkProgressByResponseItems());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"get failed. uri:-{_httpClient.BaseAddress}bulk-check");
            throw;
        }
    }

    public async Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecks(string bulkCheckDeleteUrl)
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

    public async Task<IEnumerable<IBulkExport>> LoadBulkCheckResults(string bulkCheckId, string fsmPolicy)
    {
        try
        {
            var url = $"bulk-check/{bulkCheckId}/";
            var response = await GetBulkCheckResults(url);

            if (response?.Data == null)
            {
                return Enumerable.Empty<IBulkExport>();
            }

            if (fsmPolicy == "expanded")
            {
                return response.Data.Select(x => new BulkExportTiered
                {
                    LastName = x.LastName,
                    DOB = x.DateOfBirth,
                    NI = x.NationalInsuranceNumber,
                    Outcome = x.Status.GetFsmStatusDescriptionBulkCheck(x.Tier),
                    EligibilityEndDate = x.EligibilityEndDate
                });
            }
            else
            {
                return response.Data.Select(x => new BulkExport
                {
                    LastName = x.LastName,
                    DOB = x.DateOfBirth,
                    NI = x.NationalInsuranceNumber,
                    Outcome = x.Status.GetFsmStatusDescriptionBulkCheck()
                });
            }
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, $"LoadBulkCheckResults_FsmBasic failed for bulkCheckId: {safeBulkCheckId}");
            throw;
        }
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
}