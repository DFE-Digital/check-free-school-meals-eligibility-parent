using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface ICheckGateway
{
    // bulk
    Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress(string bulkCheckUrl);
    Task<CheckEligibilityBulkResponse> GetBulkCheckResults(string resultsUrl);

    Task<CheckEligibilityResponseBulk> PostBulkCheck(CheckEligibilityRequestBulk_Fsm requestBody);

    // single
    Task<CheckEligibilityResponse> PostCheck(CheckEligibilityRequest_Fsm requestBody);
    Task<CheckEligibilityStatusResponse> GetStatus(CheckEligibilityResponse responseBody);

    // FSM Basic bulk
    Task<CheckEligibilityResponseBulk> PostBulkCheck_FsmBasic(CheckEligibilityRequestBulk_FsmBasic requestBody);
    Task<CheckEligibilityBulkStatusResponse> GetBulkCheckProgress_FsmBasic(string bulkCheckUrl);
    Task<CheckEligibilityBulkResponse> GetBulkCheckResults_FsmBasic(string resultsUrl);
    Task<CheckEligibilityBulkProgressByLAResponse> GetBulkCheckStatuses_FsmBasic(string organisationId);
    Task<CheckEligiblityBulkDeleteResponse> DeleteBulkChecksFor_FsmBasic(string bulkCheckDeleteUrl);
    Task<IEnumerable<IBulkExport>> LoadBulkCheckResults_FsmBasic(string bulkCheckId);
}