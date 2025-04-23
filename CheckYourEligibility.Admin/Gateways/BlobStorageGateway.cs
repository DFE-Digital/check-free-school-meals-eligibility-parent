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
        _connectionString = configuration["AzureStorageEvidence:ConnectionString"]; // TODO: Check this
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

            // Return only the blob name
            return uniqueBlobName;
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
            _logger.LogError(ex, $"Error deleting blob {blobName.Replace(Environment.NewLine, "")} from container {containerName.Replace(Environment.NewLine, "")}");
            throw;
        }
    }

    public async Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string blobReference, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobReference);
            
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogError($"Blob {blobReference.Replace(Environment.NewLine, "")} not found in container {containerName.Replace(Environment.NewLine, "")}");
                throw new FileNotFoundException($"File {blobReference} not found");
            }
            
            BlobProperties properties = await blobClient.GetPropertiesAsync();
            string contentType = properties.ContentType;
            
            // Download
            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return (memoryStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading blob {blobReference.Replace(Environment.NewLine, "")} from container {containerName.Replace(Environment.NewLine, "")}");
            throw;
        }
    }
}