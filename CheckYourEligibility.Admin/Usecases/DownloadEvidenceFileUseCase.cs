using CheckYourEligibility.Admin.Gateways.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.Admin.UseCases;

public interface IDownloadEvidenceFileUseCase
{
    Task<(Stream FileStream, string ContentType)> Execute(string blobReference, string containerName);
}

public class DownloadEvidenceFileUseCase : IDownloadEvidenceFileUseCase
{
    private readonly IBlobStorageGateway _blobStorageGateway;
    private readonly ILogger<DownloadEvidenceFileUseCase> _logger;

    public DownloadEvidenceFileUseCase(
        IBlobStorageGateway blobStorageGateway,
        ILogger<DownloadEvidenceFileUseCase> logger)
    {
        _blobStorageGateway = blobStorageGateway ?? throw new ArgumentNullException(nameof(blobStorageGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(Stream FileStream, string ContentType)> Execute(string blobReference, string containerName)
    {
        try
        {
            _logger.LogInformation($"Downloading file {blobReference.Replace(Environment.NewLine, "")} from blob storage container {containerName.Replace(Environment.NewLine, "")}");
            return await _blobStorageGateway.DownloadFileAsync(blobReference, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file {blobReference.Replace(Environment.NewLine, "")} from blob storage");
            throw;
        }
    }
}