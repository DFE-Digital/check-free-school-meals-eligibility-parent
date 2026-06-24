using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.Usecases
{
    public interface IGetBulkChecks
    {
        Task<IEnumerable<BulkCheck>> Execute(int organisationId);
    }

    public class GetBulkChecks : IGetBulkChecks
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<GetBulkChecks> _logger;

        public GetBulkChecks(
            ILogger<GetBulkChecks> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<IEnumerable<BulkCheck>> Execute(int organisationId)
        {
            try
            {
                
                var response = await _checkGateway.GetBulkChecks();

                if (response?.Checks == null)
                {
                    _logger.LogWarning("No bulk check statuses found for organisation: {OrganisationId}", organisationId);
                    return new List<BulkCheck>();
                }

                // Filter to only FreeSchoolMeals checks
                var fsmBasicChecks = response.Checks
                    .Where(x => x.EligibilityType == "FreeSchoolMeals" && x.Status != "Deleted")
                    .Select(MapToBulkCheck).OrderByDescending(x => x.SubmittedDate).ToList();

                _logger.LogInformation("Retrieved {Count} FSM Basic bulk checks for organisation: {OrganisationId}", 
                    fsmBasicChecks.Count, organisationId);

                // Sort by date descending
          

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
