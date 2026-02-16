using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Usecases;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class DeleteBulkCheckFileUseCase_FsmBasicTests
{
    private Mock<ILogger<DeleteBulkCheckFileUseCase_FsmBasic>> _loggerMock = null!;
    private Mock<ICheckGateway> _checkGatewayMock = null!;
    private DeleteBulkCheckFileUseCase_FsmBasic _useCase = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<DeleteBulkCheckFileUseCase_FsmBasic>>();
        _checkGatewayMock = new Mock<ICheckGateway>();
        _useCase = new DeleteBulkCheckFileUseCase_FsmBasic(_loggerMock.Object, _checkGatewayMock.Object);
    }

    #region Success Tests

    [Test]
    public async Task Execute_WithValidBulkCheckId_CallsGatewayAndReturnsSuccess()
    {
        // Arrange
        var bulkCheckId = "test-guid-123";
        var expectedResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = true,
            Message = "Deleted successfully"
        };

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic($"bulk-check/{bulkCheckId}"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Deleted successfully"));
        _checkGatewayMock.Verify(x => x.DeleteBulkChecksFor_FsmBasic($"bulk-check/{bulkCheckId}"), Times.Once);
    }

    [Test]
    public async Task Execute_WithValidBulkCheckId_LogsInformation()
    {
        // Arrange
        var bulkCheckId = "test-guid-456";
        var expectedResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = true,
            Message = "Deleted successfully"
        };

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _useCase.Execute(bulkCheckId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Successfully deleted bulk check: {bulkCheckId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Failure Tests

    [Test]
    public async Task Execute_WhenGatewayFails_ReturnsFailureResponse()
    {
        // Arrange
        var bulkCheckId = "test-guid-789";
        var expectedResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = false,
            Message = "Failed to delete"
        };

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to delete"));
    }

    [Test]
    public async Task Execute_WhenGatewayFails_LogsWarning()
    {
        // Arrange
        var bulkCheckId = "test-guid-fail";
        var expectedResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = false,
            Message = "Failed to delete"
        };

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _useCase.Execute(bulkCheckId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to delete bulk check")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Invalid Input Tests

    [Test]
    public async Task Execute_WithEmptyBulkCheckId_ReturnsFailureWithoutCallingGateway()
    {
        // Arrange
        var bulkCheckId = "";

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid bulk check ID"));
        _checkGatewayMock.Verify(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Execute_WithNullBulkCheckId_ReturnsFailureWithoutCallingGateway()
    {
        // Arrange
        string bulkCheckId = null!;

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid bulk check ID"));
        _checkGatewayMock.Verify(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Execute_WithWhitespaceBulkCheckId_ReturnsFailureWithoutCallingGateway()
    {
        // Arrange
        var bulkCheckId = "   ";

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid bulk check ID"));
        _checkGatewayMock.Verify(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Execute_WithInvalidBulkCheckId_LogsWarning()
    {
        // Arrange
        var bulkCheckId = "";

        // Act
        await _useCase.Execute(bulkCheckId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempted to delete bulk check with empty ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task Execute_WhenGatewayThrowsException_ReturnsFailureResponse()
    {
        // Arrange
        var bulkCheckId = "test-guid-exception";
        var exception = new Exception("Gateway error");

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Error deleting bulk check"));
        Assert.That(result.Message, Does.Contain("Gateway error"));
    }

    [Test]
    public async Task Execute_WhenGatewayThrowsException_LogsError()
    {
        // Arrange
        var bulkCheckId = "test-guid-exception-log";
        var exception = new Exception("Gateway error");

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .ThrowsAsync(exception);

        // Act
        await _useCase.Execute(bulkCheckId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error deleting bulk check: {bulkCheckId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region URL Format Tests

    [Test]
    public async Task Execute_ConstructsCorrectDeleteUrl()
    {
        // Arrange
        var bulkCheckId = "abc-123-def-456";
        string? capturedUrl = null;

        _checkGatewayMock
            .Setup(x => x.DeleteBulkChecksFor_FsmBasic(It.IsAny<string>()))
            .Callback<string>(url => capturedUrl = url)
            .ReturnsAsync(new CheckEligiblityBulkDeleteResponse { Success = true });

        // Act
        await _useCase.Execute(bulkCheckId);

        // Assert
        Assert.That(capturedUrl, Is.EqualTo($"bulk-check/{bulkCheckId}"));
    }

    #endregion
}
