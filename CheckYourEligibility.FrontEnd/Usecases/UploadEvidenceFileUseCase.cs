using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface IUploadEvidenceFileUseCase
{
    Task<string> Execute(IFormFile file, string containerName);
}

public class UploadEvidenceFileUseCase : IUploadEvidenceFileUseCase
{
    private readonly IBlobStorageGateway _blobStorageGateway;
    private readonly ILogger<UploadEvidenceFileUseCase> _logger;

    public UploadEvidenceFileUseCase(
        IBlobStorageGateway blobStorageGateway,
        ILogger<UploadEvidenceFileUseCase> logger)
    {
        _blobStorageGateway = blobStorageGateway ?? throw new ArgumentNullException(nameof(blobStorageGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> Execute(IFormFile file, string containerName)
    {
        try
        {
            _logger.LogInformation($"Uploading file {file.FileName.Replace(Environment.NewLine, "")} to blob storage container {containerName.Replace(Environment.NewLine, "")}");
            return await _blobStorageGateway.UploadFileAsync(file, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file {file.FileName.Replace(Environment.NewLine, "")} to blob storage");
            throw;
        }
    }
}