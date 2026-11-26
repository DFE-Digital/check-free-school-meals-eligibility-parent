using System.Net;
using CheckYourEligibility.FrontEnd.Gateways;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace CheckYourEligibility.FrontEnd.Tests.Gateways;

public class BaseGatewayTests
{
    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public async Task ApiError_LogsSafeStructuredDetailsAndThrowsReference(bool hasRequestBody, bool hasResponseBody)
    {
        var logger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"token\",\"expires_in\":3600}")
            });
        using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };
        var gateway = new TestGateway(loggerFactory.Object, client, Mock.Of<IConfiguration>());
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        if (hasResponseBody)
            response.Content = new StringContent("response-secret");

        var act = () => gateway.LogFailure(response, "POST",
            "https://example.test/person-secret?query=query-secret", "client_secret=request-secret", hasRequestBody);

        var exception = (await act.Should().ThrowExactlyAsync<Exception>()).Which;
        var log = logger.Invocations.Should().ContainSingle(x => x.Method.Name == nameof(ILogger.Log)).Subject;
        log.Arguments[0].Should().Be(LogLevel.Error);
        log.Arguments[3].Should().BeNull();
        var properties = ((IReadOnlyList<KeyValuePair<string, object?>>)log.Arguments[2])
            .ToDictionary(x => x.Key, x => x.Value);
        properties["Method"].Should().Be("POST");
        properties["StatusCode"].Should().Be(400);
        var logId = properties["LogId"].Should().BeOfType<string>().Subject;
        Guid.TryParse(logId, out _).Should().BeTrue();
        exception.Message.Should().Contain(logId);
        var loggedValues = string.Join(" ", properties.Values);
        loggedValues.Should().NotContain("person-secret").And.NotContain("query-secret")
            .And.NotContain("request-secret").And.NotContain("response-secret");
    }

    [Test]
    public void AuthorisationFailure_DoesNotLogCredentialsOrResponseBody()
    {
        var logger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["Api:AuthorisationUsername"]).Returns("client-id-secret");
        configuration.Setup(x => x["Api:AuthorisationPassword"]).Returns("client-password-secret");
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("response-secret")
            });
        using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };

        _ = new TestGateway(loggerFactory.Object, client, configuration.Object);

        var logs = logger.Invocations.Where(x => x.Method.Name == nameof(ILogger.Log)).ToList();
        logs.Should().HaveCount(2);
        var loggedValues = string.Join(" ", logs.SelectMany(x => x.Arguments).Select(x => x?.ToString()));
        loggedValues.Should().NotContain("client-id-secret").And.NotContain("client-password-secret")
            .And.NotContain("response-secret");
        logs.Should().Contain(x => x.Arguments[2].ToString()!.Contains("StatusCode: 400"));
    }

    private sealed class TestGateway : BaseGateway
    {
        public TestGateway(ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration)
            : base("EcsService", logger, httpClient, configuration, null)
        {
        }

        public Task LogFailure(HttpResponseMessage response, string method, string uri, string data, bool hasRequestBody)
        {
            return hasRequestBody
                ? LogApiErrorInternal(response, method, uri, data)
                : LogApiErrorInternal(response, method, uri);
        }
    }
}
