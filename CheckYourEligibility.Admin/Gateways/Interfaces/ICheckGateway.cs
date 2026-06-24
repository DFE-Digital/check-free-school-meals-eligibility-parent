using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface ICheckGateway
{
    // bulk
    Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl);
    Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);
    Task<CheckEligibilityBulkProgressByResponseItems> GetBulkChecks();
    Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecks(string bulkCheckDeleteUrl);
    Task<IEnumerable<TBulkExport>> LoadBulkCheckResults<TBulkExport>(string bulkCheckId) where TBulkExport : IBulkExport;
    Task<CheckEligibilityResponseBulk> PostBulkCheck<TBulk>(TBulk requestBody) where TBulk : CheckEligibilityRequestBulkBase;
    Task<EstablishmentResponse> GetAcademiesAsync(int multiAcademyTrustId);
    Task<EstablishmentResponse> GetSchoolsAsync(int localAuthorityId);

    // single
    Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Enhanced requestBody);
    Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);
    Task<CheckEligibilityItemResponse> GetCheck(CheckEligibilityResponse responseBody);



    // Reports
    Task<EligibilityCheckReportResponse> GenerateEligibilityCheckReport(EligibilityCheckReportRequest requestBody);
    Task<EligibilityCheckReportHistoryResponse> GetEligibilityCheckReportHistory(string localAuthorityId, int pageNumber);
}