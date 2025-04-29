using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class EvidenceFileUseCaseTests
{
    // Upload evidence file use case tests
    [TestFixture]
    public class UploadEvidenceFileUseCaseTests
    {
        private Mock<IBlobStorageGateway> _mockBlobStorageGateway;
        private Mock<ILogger<UploadEvidenceFileUseCase>> _mockLogger;
        private UploadEvidenceFileUseCase _sut;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockBlobStorageGateway = new Mock<IBlobStorageGateway>();
            _mockLogger = new Mock<ILogger<UploadEvidenceFileUseCase>>();
            _sut = new UploadEvidenceFileUseCase(_mockBlobStorageGateway.Object, _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_WithValidFile_ShouldUploadSuccessfully()
        {
            // Arrange
            var mockFile = CreateMockFile("test-file.pdf", "application/pdf", "test content");
            var containerName = "evidence";
            var expectedBlobReference = "blob-reference";

            _mockBlobStorageGateway
                .Setup(x => x.UploadFileAsync(mockFile.Object, containerName))
                .ReturnsAsync(expectedBlobReference);

            // Act
            var result = await _sut.Execute(mockFile.Object, containerName);

            // Assert
            result.Should().Be(expectedBlobReference);
            _mockBlobStorageGateway.Verify(x => x.UploadFileAsync(mockFile.Object, containerName), Times.Once);
        }

        [Test]
        public void Execute_WhenUploadThrowsException_ShouldPropagateException()
        {
            // Arrange
            var mockFile = CreateMockFile("test-file.pdf", "application/pdf", "test content");
            var containerName = "evidence";
            var expectedException = new Exception("Upload failed");

            _mockBlobStorageGateway
                .Setup(x => x.UploadFileAsync(mockFile.Object, containerName))
                .ThrowsAsync(expectedException);

            // Act & Assert
            Func<Task> act = async () => await _sut.Execute(mockFile.Object, containerName);
            act.Should().ThrowAsync<Exception>().WithMessage("Upload failed");
            
            _mockBlobStorageGateway.Verify(x => x.UploadFileAsync(mockFile.Object, containerName), Times.Once);
            VerifyLoggerError("Error uploading file test-file.pdf to blob storage");
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string contentType, string content)
        {
            var mockFile = new Mock<IFormFile>();
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(contentBytes);
            
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(contentBytes.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            
            return mockFile;
        }

        private void VerifyLoggerError(string contains)
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(contains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Download evidence file use case tests
    [TestFixture]
    public class DownloadEvidenceFileUseCaseTests
    {
        private Mock<IBlobStorageGateway> _mockBlobStorageGateway;
        private Mock<ILogger<DownloadEvidenceFileUseCase>> _mockLogger;
        private DownloadEvidenceFileUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _mockBlobStorageGateway = new Mock<IBlobStorageGateway>();
            _mockLogger = new Mock<ILogger<DownloadEvidenceFileUseCase>>();
            _sut = new DownloadEvidenceFileUseCase(_mockBlobStorageGateway.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Execute_WithValidBlobReference_ShouldReturnFileStream()
        {
            // Arrange
            var blobReference = "test-blob-reference";
            var containerName = "evidence";
            var expectedContentType = "application/pdf";
            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

            _mockBlobStorageGateway
                .Setup(x => x.DownloadFileAsync(blobReference, containerName))
                .ReturnsAsync((expectedStream, expectedContentType));

            // Act
            var result = await _sut.Execute(blobReference, containerName);

            // Assert
            result.FileStream.Should().BeSameAs(expectedStream);
            result.ContentType.Should().Be(expectedContentType);
            _mockBlobStorageGateway.Verify(x => x.DownloadFileAsync(blobReference, containerName), Times.Once);
        }

        [Test]
        public void Execute_WhenDownloadThrowsException_ShouldPropagateException()
        {
            // Arrange
            var blobReference = "test-blob-reference";
            var containerName = "evidence";
            var expectedException = new Exception("Download failed");

            _mockBlobStorageGateway
                .Setup(x => x.DownloadFileAsync(blobReference, containerName))
                .ThrowsAsync(expectedException);

            // Act & Assert
            Func<Task> act = async () => await _sut.Execute(blobReference, containerName);
            act.Should().ThrowAsync<Exception>().WithMessage("Download failed");
            
            _mockBlobStorageGateway.Verify(x => x.DownloadFileAsync(blobReference, containerName), Times.Once);
            VerifyLoggerError("Error downloading file");
        }

        private void VerifyLoggerError(string contains)
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(contains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Delete evidence file use case tests
    [TestFixture]
    public class DeleteEvidenceFileUseCaseTests
    {
        private Mock<IBlobStorageGateway> _mockBlobStorageGateway;
        private Mock<ILogger<DeleteEvidenceFileUseCase>> _mockLogger;
        private DeleteEvidenceFileUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _mockBlobStorageGateway = new Mock<IBlobStorageGateway>();
            _mockLogger = new Mock<ILogger<DeleteEvidenceFileUseCase>>();
            _sut = new DeleteEvidenceFileUseCase(_mockBlobStorageGateway.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Execute_WithValidBlobReference_ShouldDeleteSuccessfully()
        {
            // Arrange
            var blobReference = "test-blob-reference";
            var containerName = "evidence";

            _mockBlobStorageGateway
                .Setup(x => x.DeleteFileAsync(blobReference, containerName))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.Execute(blobReference, containerName);

            // Assert
            _mockBlobStorageGateway.Verify(x => x.DeleteFileAsync(blobReference, containerName), Times.Once);
        }

        [Test]
        public void Execute_WhenDeleteThrowsException_ShouldPropagateException()
        {
            // Arrange
            var blobReference = "test-blob-reference";
            var containerName = "evidence";
            var expectedException = new Exception("Delete failed");

            _mockBlobStorageGateway
                .Setup(x => x.DeleteFileAsync(blobReference, containerName))
                .ThrowsAsync(expectedException);

            // Act & Assert
            Func<Task> act = async () => await _sut.Execute(blobReference, containerName);
            act.Should().ThrowAsync<Exception>().WithMessage("Delete failed");
            
            _mockBlobStorageGateway.Verify(x => x.DeleteFileAsync(blobReference, containerName), Times.Once);
            VerifyLoggerError("Error deleting file");
        }

        private void VerifyLoggerError(string contains)
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(contains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}