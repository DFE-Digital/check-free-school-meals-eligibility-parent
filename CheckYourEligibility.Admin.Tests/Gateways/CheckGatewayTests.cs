using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace CheckYourEligibility.Admin.Gateways.Tests.Check
{
    internal class CheckGatewayTests
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private DerivedCheckGateway _sut;

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

            _sut = new DerivedCheckGateway(_loggerFactoryMock.Object, _httpClient, _configMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }


        [Test]
        public async Task Given_GetStatus_When_CalledWithValidResponse_Should_ReturnCheckEligibilityStatusResponse()
        {
            // Arrange
            var responseBody = new CheckEligibilityResponse
            {
                Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
            };
            var responseContent = new CheckEligibilityStatusResponse();
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
            var result = await _sut.GetStatus(responseBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task Given_PostCheck_When_CalledWithValidRequest_Should_ReturnCheckEligibilityResponse()
        {
            // Arrange
            var requestBody = new CheckEligibilityRequest_Fsm();
            var responseContent = new CheckEligibilityResponse();
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
            var result = await _sut.PostCheck(requestBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public void Given_GetStatus_When_CalledWithNullInput_Should_ThrowArgumentNullException()
        {
            // Act
            var result = async () => await _sut.GetStatus(null);


            // Assert
            var resultAsResponse = result.As<Task<CheckEligibilityStatusResponse>>();
            resultAsResponse.Should().BeNull();

            result.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task Given_GetStatus_When_GivenInAnInvalidValue_ShouldReturnNull()
        {
            // Arrange

            // Assert
            var result = _sut.GetStatus(new CheckEligibilityResponse()
            {
                Data = new StatusValue()
                {
                    Status = "unknown"
                },
                Links = new CheckEligibilityResponseLinks()
                {
                    Get_EligibilityCheck = "link"
                }
            });

            // Act
            result.Result.Should().Be(null);

        }

        [Test]
        public async Task Given_PostCheck_When_ApiReturnsUnauthorized_Should_LogApiErrorAnd_Throw_UnauthorizedAccessException()
        {
            // Arrange
            var requestBody = new CheckEligibilityRequest_Fsm();
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
            Func<Task> act = async () => await _sut.PostCheck(requestBody);

            // Assert
            await act.Should().ThrowExactlyAsync<UnauthorizedAccessException>();

        }


        [Test]
        public async Task Given_GetStatus_When_ApiReturnsUnauthorized_Should_LogApiErrorAndThrowExecption()
        {
            // Arrange
            var responseBody = new CheckEligibilityResponse
            {
                Links = new CheckEligibilityResponseLinks { Get_EligibilityCheck = "EligibilityCheckLink" }
            };
            var responseContent = new CheckEligibilityStatusResponse();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = _sut.GetStatus(responseBody);

            // Assert
            result.Result.Data.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);
        }
    }
}
