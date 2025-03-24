using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces
{
    public interface ICheckGateway
    {
        // bulk
        Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl);
        Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);
        Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk_Fsm requestBody);
        // single
        Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Fsm requestBody);
        Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);  
    }
}