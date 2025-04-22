using CheckYourEligibility.Admin.Gateways.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.Admin.UseCases;

public interface IUploadEvidenceFileUseCase
{
    Task<string> Execute(IFormFile file, string containerName);
}

public interface IDownloadEvidenceFileUseCase
{
    Task<(Stream FileStream, string ContentType)> Execute(string blobReference, string containerName);
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
            _logger.LogInformation("Uploading file {FileName} to blob storage container {ContainerName}", file.FileName, containerName);
            return await _blobStorageGateway.UploadFileAsync(file, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to blob storage", file.FileName);
            throw;
        }
    }
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
            _logger.LogInformation("Downloading file {BlobReference} from blob storage container {ContainerName}", blobReference, containerName);
            return await _blobStorageGateway.DownloadFileAsync(blobReference, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {BlobReference} from blob storage", blobReference);
            throw;
        }
    }
}