using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
internal class BaseControllerTests : TestBase
{
    private Mock<IDfeSignInApiService> _dfeSignInApiServiceMock;
    private Mock<ISchoolMenuContextResolver> _schoolMenuContextResolverMock;
    private Mock<ILocalAuthoritySettingsGateway> _localAuthoritySettingsGatewayMock;
    private TestableBaseController _sut;

    [SetUp]
    public void SetUpBaseControllerTests()
    {
        _dfeSignInApiServiceMock = new Mock<IDfeSignInApiService>();
        _dfeSignInApiServiceMock
            .Setup(s => s.GetUserRolesAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<Role>());

        _schoolMenuContextResolverMock = new Mock<ISchoolMenuContextResolver>();
        _localAuthoritySettingsGatewayMock = new Mock<ILocalAuthoritySettingsGateway>();
        _sut = new TestableBaseController(
            _dfeSignInApiServiceMock.Object,
            _schoolMenuContextResolverMock.Object,
            _localAuthoritySettingsGatewayMock.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext.Object
        };
        _sut.GetDfeClaimsAsync().Wait();
    }

    [TearDown]
    public void TearDownBaseControllerTests()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task GetFreeSchoolMealsPolicy_WhenNotCached_CallsGatewayAndCachesResult()
    {
        // Arrange
        var expectedPolicy = new EligibilityPolicyAssignment
        {
            CheckType = CheckEligibilityType.FreeSchoolMeals.ToString(),
            EligibilityCriteria = EligibilityCriteria.expanded
        };
        var settingsResponse = new LocalAuthoritySettingsResponse
        {
            EligibilityPolicies = [expectedPolicy]
        };
        _localAuthoritySettingsGatewayMock
            .Setup(g => g.GetLocalAuthoritySettingsAsync(It.IsAny<int>()))
            .ReturnsAsync(settingsResponse);

        // Act
        EligibilityPolicyAssignment policy = await _sut.GetFreeSchoolMealsPolicy();

        // Assert
        _localAuthoritySettingsGatewayMock.Verify(g => g.GetLocalAuthoritySettingsAsync(It.IsAny<int>()), Times.Once);
        _sessionMock.Verify(s => s.Set(It.Is<string>(k => k == "FreeSchoolMealsPolicy"), It.IsAny<byte[]>()), Times.Once);
        policy.Should().BeEquivalentTo(expectedPolicy);
    }

    [Test]
    public async Task GetFreeSchoolMealsPolicy_WhenCached_ReturnsCachedPolicyWithoutCallingGateway()
    {
        // Arrange
        var cachedPolicy = new EligibilityPolicyAssignment
        {
            CheckType = CheckEligibilityType.FreeSchoolMeals.ToString(),
            EligibilityCriteria = EligibilityCriteria.expanded
        };
        var serializedPolicy = JsonConvert.SerializeObject(cachedPolicy);
        _sessionMock.Object.SetString("FreeSchoolMealsPolicy", serializedPolicy);

        // Act
        EligibilityPolicyAssignment policy = await _sut.GetFreeSchoolMealsPolicy();

        // Assert
        _localAuthoritySettingsGatewayMock.Verify(g => g.GetLocalAuthoritySettingsAsync(It.IsAny<int>()), Times.Never);
        policy.Should().BeEquivalentTo(cachedPolicy);
    }

    [Test]
    public async Task GetFreeSchoolMealsPolicy_WhenGatewayThrows_ReturnsDefaultStandardPolicy()
    {
        // Arrange
        _localAuthoritySettingsGatewayMock
            .Setup(g => g.GetLocalAuthoritySettingsAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("API failure"));

        // Act
        var policy = await _sut.GetFreeSchoolMealsPolicy();

        // Assert
        policy.Should().NotBeNull();
        policy.CheckType.Should().Be(CheckEligibilityType.FreeSchoolMeals.ToString());
        policy.EligibilityCriteria.Should().Be(EligibilityCriteria.standard);
    }

    [Test]
    public async Task IsExpandedFSMEnabled_WhenPolicyIsExpanded_ReturnsTrue()
    {
        // Arrange
        var expectedPolicy = new EligibilityPolicyAssignment
        {
            CheckType = CheckEligibilityType.FreeSchoolMeals.ToString(),
            EligibilityCriteria = EligibilityCriteria.expanded
        };
        var settingsResponse = new LocalAuthoritySettingsResponse
        {
            EligibilityPolicies = [expectedPolicy]
        };

        _localAuthoritySettingsGatewayMock
            .Setup(g => g.GetLocalAuthoritySettingsAsync(It.IsAny<int>()))
            .ReturnsAsync(settingsResponse);

        // Act
        var result = await _sut.IsExpandedFSMEnabled();

        // Assert
        result.Should().BeTrue();
    }

    private sealed class TestableBaseController : BaseController
    {
        public TestableBaseController(
            IDfeSignInApiService dfeSignInApiService,
            ISchoolMenuContextResolver schoolMenuContextResolver,
            ILocalAuthoritySettingsGateway localAuthoritySettingsGateway)
            : base(dfeSignInApiService, schoolMenuContextResolver, localAuthoritySettingsGateway)
        {
        }
    }
}