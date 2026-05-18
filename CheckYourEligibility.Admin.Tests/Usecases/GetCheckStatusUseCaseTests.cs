using System.Text;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class GetCheckStatusUseCaseTests
{
    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GetCheckStatusUseCase>>();
        _checkGatewayMock = new Mock<ICheckGateway>();
        _sessionMock = new Mock<ISession>();
        _sut = new GetCheckStatusUseCase(
            _loggerMock.Object,
            _checkGatewayMock.Object
        );
    }

    private Mock<ILogger<GetCheckStatusUseCase>> _loggerMock;
    private Mock<ICheckGateway> _checkGatewayMock;
    private Mock<ISession> _sessionMock;
    private GetCheckStatusUseCase _sut;

    public static object[] StatusTestCases =
    {
        new object[] { "eligible", null, "2027-01-01"},
        new object[] { "eligible", "targeted", "2027-01-01"},
        new object[] { "notEligible", "expanded", "2027-01-01" },
        new object[] { "parentNotFound", null, null },
        new object[] { "error", null, null },
        new object[] { "queuedForProcessing", null, null }
    };

    [TestCaseSource(nameof(StatusTestCases))]
    public async Task Execute_WithValidStatus_ShouldReturnCorrectViewAndModel(
        string status, string tier, string eligibilityEndDate)
    {
        // Arrange
        var response = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = status, Tier = tier, EligibilityEndDate = eligibilityEndDate }
        };
        var responseJson = JsonConvert.SerializeObject(response);
        var statusResponse = new CheckEligibilityStatusResponse
        {
            Data = new StatusValue { Status = status, Tier = tier, EligibilityEndDate = eligibilityEndDate }
        };

        var expectedOutcome = new StatusValue { Status = status, Tier = tier, EligibilityEndDate = eligibilityEndDate };

        _checkGatewayMock
            .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
            .ReturnsAsync(statusResponse);

        // Act
        var outcome = await _sut.Execute(responseJson, _sessionMock.Object);

        // Assert
        outcome.Should().BeEquivalentTo(expectedOutcome);
        _sessionMock.Verify(s =>
                s.Set("CheckResult", It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b) == status)),
            Times.Once);
    }

    [Test]
    public async Task Execute_WithEmptyResponse_ShouldReturnTechnicalError()
    {
        // Act
        await FluentActions.Invoking(() =>
                _sut.Execute(
                    null, _sessionMock.Object))
            .Should().ThrowAsync<Exception>()
            .WithMessage("No response data found in TempData.");
    }

    [Test]
    public async Task Execute_WithNullCheckResponse_ShouldReturnTechnicalError()
    {
        // Arrange
        var response = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = "any" }
        };
        var responseJson = JsonConvert.SerializeObject(response);

        _checkGatewayMock
            .Setup(x => x.GetStatus(It.IsAny<CheckEligibilityResponse>()))
            .ReturnsAsync((CheckEligibilityStatusResponse)null);

        // Act
        await FluentActions.Invoking(() =>
                _sut.Execute(
                    responseJson, _sessionMock.Object))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Null response received from GetStatus.");
    }
}