using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Usecases
{
    public interface IDeleteBulkCheckFileUseCase_FsmBasic
    {
        Task<CheckEligiblityBulkDeleteResponse> Execute(string bulkCheckId);
    }

    public class DeleteBulkCheckFileUseCase_FsmBasic : IDeleteBulkCheckFileUseCase_FsmBasic
    {
        private readonly ICheckGateway _checkGateway;
        private readonly ILogger<DeleteBulkCheckFileUseCase_FsmBasic> _logger;

        public DeleteBulkCheckFileUseCase_FsmBasic(
            ILogger<DeleteBulkCheckFileUseCase_FsmBasic> logger,
            ICheckGateway checkGateway)
        {
            _logger = logger;
            _checkGateway = checkGateway;
        }

        public async Task<CheckEligiblityBulkDeleteResponse> Execute(string bulkCheckId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bulkCheckId))
                {
                    _logger.LogWarning("Attempted to delete bulk check with empty ID");
                    return new CheckEligiblityBulkDeleteResponse
                    {
                        Success = false,
                        Message = "Invalid bulk check ID"
                    };
                }

                var deleteUrl = $"bulk-check/{bulkCheckId}";
                var response = await _checkGateway.DeleteBulkChecksFor_FsmBasic(deleteUrl);

                var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
                if (response.Success)
                {
                    _logger.LogInformation("Successfully deleted bulk check: {BulkCheckId}", safeBulkCheckId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete bulk check: {BulkCheckId}. Message: {Message}", 
                        safeBulkCheckId, response.Message);
                }

                return response;
            }
            catch (Exception ex)
            {
                var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
                _logger.LogError(ex, "Error deleting bulk check: {BulkCheckId}", safeBulkCheckId);
                return new CheckEligiblityBulkDeleteResponse
                {
                    Success = false,
                    Message = $"Error deleting bulk check: {ex.Message}"
                };
            }
        }
    }
}
