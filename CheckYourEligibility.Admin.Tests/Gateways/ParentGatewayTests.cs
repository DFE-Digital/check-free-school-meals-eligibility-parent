using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace CheckYourEligibility.Admin.Gateways.Tests.Parent
{
    public class ParentGatewayTests
    {
        private Mock<ILoggerFactory> _loggerFactoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private DerivedParentGateway _sut;

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

            _sut = new DerivedParentGateway(_loggerFactoryMock.Object, _httpClient, _configMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task Given_GetSchool_When_CalledWithValidQuery_Should_ReturnSchoolSearchResponse()
        {
            // Arrange
            var query = "Test";
            var responseContent = new EstablishmentSearchResponse();
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
            var result = await _sut.GetSchool(query);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }

        [Test]
        public async Task Given_PostApplication_When_CalledWithValidRequest_Should_ReturnApplicationSaveItemResponse()
        {
            // Arrange
            var requestBody = new ApplicationRequest() { Data = new ApplicationRequestData() };
            var responseContent = new ApplicationSaveItemResponse();
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
            var result = await _sut.PostApplication_Fsm(requestBody);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(responseContent);
        }



        [Test]
        public async Task Given_GetSchool_When_ApiReturnsNotFound_Should_ReturnNullAndLogAPIError()
        {
            // Arrange
            var query = "Test";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.GetSchool(query);

            // Assert
            result.Data.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);

        }

        [Test]
        public async Task Given_PostApplication_When_ApiReturnsServerError_Should_ReturnNullAndLogAPIError()
        {
            // Arrange
            var requestBody = new ApplicationRequest() { Data = new ApplicationRequestData()};
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _sut.PostApplication_Fsm(requestBody);

            // Assert
            result.Data.Should().BeNull();
            result.Links.Should().BeNull();
            _sut.apiErrorCount.Should().Be(1);
        }
    }
}
