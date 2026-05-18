using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces
{
    public interface IEligibilityCheckReportingGateway
    {
        Task<EligibilityCheckReportResponse> GenerateEligibilityCheckReport(EligibilityCheckReportRequest requestBody);
        Task<EligibilityCheckReportHistoryResponse> GetEligibilityCheckReportHistory(string localAuthorityId, int pageNumber);
        Task DeleteEligibilityCheckReport(Guid reportId);
    }
}
