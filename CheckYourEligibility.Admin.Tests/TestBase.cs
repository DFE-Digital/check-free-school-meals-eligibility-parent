using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using AutoFixture;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Tests.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CheckYourEligibility.Admin.Tests;

[ExcludeFromCodeCoverage]
public abstract class TestBase
{
    public Mock<IConfiguration> _configMock;
    public Fixture _fixture = new();
    public Mock<HttpContext> _httpContext;
    public Mock<ISession> _sessionMock;
    public ITempDataDictionary _tempData;
    public Mock<ClaimsPrincipal> _userMock;

    [SetUp]
    public void SetUp()
    {
        SetUpClaimsData();
        SetUpTempData();
        SetUpSessionData();
        SetUpHTTPContext();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(x => x["BulkUploadAttemptLimit"]).Returns("10");
        _configMock.Setup(x => x["BulkEligibilityCheckLimit"]).Returns("250");
    }

    protected void SetUpTempData()
    {
        var tempDataProvider = Mock.Of<ITempDataProvider>();
        var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        _tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
    }

    protected void SetUpSessionData()
    {
        _sessionMock = new Mock<ISession>();
        var sessionStorage = new Dictionary<string, byte[]>();

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });
    }

    protected void SetUpHTTPContext()
    {
        _httpContext = new Mock<HttpContext>();
        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        _httpContext.Setup(ctx => ctx.User).Returns(_userMock.Object);
    }

    protected void SetUpClaimsData()
    {
        _userMock = new Mock<ClaimsPrincipal>();
        var claimSchool = new Claim("organisation", Resources.ClaimSchool);
        _userMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            claimSchool,
            new($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "123"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "testFirstName"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "testSurname")
        });
    }
}