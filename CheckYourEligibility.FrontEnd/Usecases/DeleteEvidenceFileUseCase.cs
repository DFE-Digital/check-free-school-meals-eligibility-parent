using CheckYourEligibility.FrontEnd.Gateways.Interfaces;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface IDeleteEvidenceFileUseCase
{
    Task Execute(string blobReference, string containerName);
}

public class DeleteEvidenceFileUseCase : IDeleteEvidenceFileUseCase
{
    private readonly IBlobStorageGateway _blobStorageGateway;
    private readonly ILogger<DeleteEvidenceFileUseCase> _logger;

    public DeleteEvidenceFileUseCase(
        IBlobStorageGateway blobStorageGateway,
        ILogger<DeleteEvidenceFileUseCase> logger)
    {
        _blobStorageGateway = blobStorageGateway ?? throw new ArgumentNullException(nameof(blobStorageGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(string blobReference, string containerName)
    {
        try
        {
            _logger.LogInformation($"Deleting file {blobReference.Replace(Environment.NewLine, "")} from blob storage container {containerName.Replace(Environment.NewLine, "")}");
            await _blobStorageGateway.DeleteFileAsync(blobReference, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {blobReference.Replace(Environment.NewLine, "")} from blob storage");
            throw;
        }
    }
}