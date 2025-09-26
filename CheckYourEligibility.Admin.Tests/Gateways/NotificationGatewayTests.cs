using System.Net;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Gateways.Tests.Notification;

[TestFixture]
public class NotificationGatewayTests
{
    private Mock<IConfiguration> _configMock;
    private HttpClient _httpClient;
    private IHttpContextAccessor _httpContextAccessor;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<ILoggerFactory> _loggerFactoryMock;
    private Mock<ILogger> _loggerMock;
    private DerivedNotificationGateway _sut;

    [SetUp]
    public void Setup()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(x => x["Api:AuthorisationUsername"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationPassword"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationEmail"]).Returns("SomeValue");
        _configMock.Setup(x => x["Api:AuthorisationScope"]).Returns("SomeValue");

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7000")
        };

        _sut = new DerivedNotificationGateway(_loggerFactoryMock.Object, _httpClient, _configMock.Object, _httpContextAccessor);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task Given_SendNotification_When_CalledWithValidRequest_Should_ReturnNotificationItemResponse()
    {
        // Arrange
        var requestBody = new NotificationRequest 
        { 
            Data = new NotificationRequestData 
            { 
                Email = "test@example.com",
                Type = NotificationType.ParentApplicationSuccessful,
                Personalisation = new Dictionary<string, object>
                {
                    { "name", "Test User" },
                    { "application_id", "APP123" }
                }
            } 
        };
        
        var responseContent = new NotificationItemResponse();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(responseContent))
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.SendNotification(requestBody);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseContent);
    }

    [Test]
    public async Task Given_SendNotification_When_ApiReturnsUnauthorized_Should_LogApiErrorAnd_Throw_UnauthorizedAccessException()
    {
        // Arrange
        var requestBody = new NotificationRequest
        {
            Data = new NotificationRequestData
            {
                Email = "test@example.com",
                Type = NotificationType.ParentApplicationSuccessful
            }
        };
        
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.SendNotification(requestBody);

        // Assert
        await act.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Given_SendNotification_When_ApiReturnsServerError_Should_LogApiErrorAndRethrowException()
    {
        // Arrange
        var requestBody = new NotificationRequest
        {
            Data = new NotificationRequestData
            {
                Email = "test@example.com",
                Type = NotificationType.ParentApplicationEvidenceSent
            }
        };

        // Setup the mock to throw an exception when SendAsync is called with any request
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated server error"));

        // Act
        Func<Task> act = async () => await _sut.SendNotification(requestBody);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _sut.apiErrorCount.Should().Be(0);
    }
}