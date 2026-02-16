using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.Usecases
{
    public interface IGetBulkCheckStatusesUseCase_FsmBasic
    {
        Task<IEnumerable<BulkCheck>> Execute(string organisationId);
    }

    public class GetBulkCheckStatusesUseCase_FsmBasic : IGetBulkCheckStatusesUseCase_FsmBasic
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<GetBulkCheckStatusesUseCase_FsmBasic> _logger;

        public GetBulkCheckStatusesUseCase_FsmBasic(
            ILogger<GetBulkCheckStatusesUseCase_FsmBasic> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<IEnumerable<BulkCheck>> Execute(string organisationId)
        {
            try
            {
                var response = await _checkGateway.GetBulkCheckStatuses_FsmBasic(organisationId);

                if (response?.Checks == null)
                {
                    _logger.LogWarning("No bulk check statuses found for organisation: {OrganisationId}", organisationId);
                    return new List<BulkCheck>();
                }

                // Filter to only FreeSchoolMeals checks
                var fsmBasicChecks = response.Checks
                    .Where(x => x.EligibilityType == "FreeSchoolMeals")
                    .Select(MapToBulkCheck)
                    .ToList();

                _logger.LogInformation("Retrieved {Count} FSM Basic bulk checks for organisation: {OrganisationId}", 
                    fsmBasicChecks.Count, organisationId);

                return fsmBasicChecks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bulk check statuses for organisation: {OrganisationId}", organisationId);
                throw;
            }
        }

        private BulkCheck MapToBulkCheck(CheckEligibilityBulkProgressResponse response)
        {
            return new BulkCheck
            {
                BulkCheckId = response.Id,
                Status = MapStatus(response.Status),
                SubmittedDate = response.SubmittedDate,
                SubmittedBy = response.SubmittedBy,
                EligibilityType = response.EligibilityType,
                Filename = response.Filename,
                NumberOfRecords = response.NumberOfRecords,
                FinalNameInCheck = response.FinalNameInCheck
            };
        }

        private string MapStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "completed" => "Completed",
                "inprogress" => "In progress",
                "notstarted" => "Not started",
                "failed" => "Failed",
                _ => status ?? "Unknown"
            };
        }
    }
}
