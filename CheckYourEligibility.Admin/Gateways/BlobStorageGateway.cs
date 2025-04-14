using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Gateways;

public class BlobStorageGateway : IBlobStorageGateway
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageGateway> _logger;
    private readonly string _connectionString;

    public BlobStorageGateway(IConfiguration configuration, ILoggerFactory logger)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"]; // TODO: Check this
        _blobServiceClient = new BlobServiceClient(_connectionString);
        _logger = logger.CreateLogger<BlobStorageGateway>();
    }

    public async Task<string> UploadFileAsync(IFormFile file, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            string uniqueBlobName = $"{Guid.NewGuid()}-{file.FileName}";
            var blobClient = containerClient.GetBlobClient(uniqueBlobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });
            }

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to blob storage: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string blobName, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobName} from container {ContainerName}", blobName, containerName);
            throw;
        }
    }
}