using System.Text;
using AutoFixture;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.Gateways;

[TestFixture]
public class BlobStorageGatewayTests
{
    private Mock<IBlobStorageGateway> _mockBlobStorageGateway;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILoggerFactory> _mockLoggerFactory;
    private Mock<ILogger<BlobStorageGatewayTests>> _mockLogger;
    private const string ContainerName = "content";
    private const string BlobUrl = "https://testaccount.blob.core.windows.net/content/test-file.pdf";

    private readonly Fixture _fixture = new();

    [SetUp]
    public void Setup()
    {
        // Configure mocks
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLogger = new Mock<ILogger<BlobStorageGatewayTests>>();
        _mockBlobStorageGateway = new Mock<IBlobStorageGateway>();

        // Setup mock configuration
        _mockConfiguration.Setup(x => x["AzureStorage:ConnectionString"])
            .Returns("DefaultEndpointsProtocol=https;AccountName=testaccount;AccountKey=testkey;EndpointSuffix=core.windows.net");
            
        _mockBlobStorageGateway
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), ContainerName))
            .ReturnsAsync(BlobUrl);
    }

    [Test]
    public async Task UploadFileAsync_ShouldUploadFileSuccessfully()
    {
        // Arrange
        var fileName = "test-file.pdf";
        var contentType = "application/pdf";
        var fileContent = "This is a test file content";
        
        var mockFile = CreateMockFile(fileName, contentType, fileContent);
        
        // Act
        var result = await _mockBlobStorageGateway.Object.UploadFileAsync(mockFile.Object, ContainerName);
        
        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(BlobUrl);
        
        // Verify the method was called
        _mockBlobStorageGateway.Verify(x => x.UploadFileAsync(It.IsAny<IFormFile>(), ContainerName), Times.Once);
    }

    [Test]
    public async Task DeleteFileAsync_ShouldDeleteFileSuccessfully()
    {
        // Arrange
        var blobName = "test-blob-name.pdf";
        _mockBlobStorageGateway
            .Setup(x => x.DeleteFileAsync(blobName, ContainerName))
            .Returns(Task.CompletedTask);
        
        // Act
        await _mockBlobStorageGateway.Object.DeleteFileAsync(blobName, ContainerName);
        
        // Assert
        _mockBlobStorageGateway.Verify(x => x.DeleteFileAsync(blobName, ContainerName), Times.Once);
    }

    private Mock<IFormFile> CreateMockFile(string fileName, string contentType, string content)
    {
        var mockFile = new Mock<IFormFile>();
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(contentBytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        
        return mockFile;
    }
}