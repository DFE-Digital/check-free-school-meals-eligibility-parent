using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IBlobStorageGateway
{
    Task<string> UploadFileAsync(IFormFile file, string containerName);
    Task DeleteFileAsync(string blobName, string containerName);
    Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string blobReference, string containerName);
}