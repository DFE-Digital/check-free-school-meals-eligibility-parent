using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Security.Claims;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
internal class HomeControllerTests : TestBase
{
    private Mock<IDfeSignInApiService> _mockDfeSignInApiService;
    private Mock<ILocalAuthoritySettingsGateway> _mockLocalAuthoritySettingsGateway;
    private IMemoryCache _memoryCache;
    private HomeController _sut;

    [SetUp]
    public void SetUp()
    {
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        _mockLocalAuthoritySettingsGateway = new Mock<ILocalAuthoritySettingsGateway>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _sut = new HomeController(
            _mockDfeSignInApiService.Object,
            _mockLocalAuthoritySettingsGateway.Object,
            _memoryCache);

        base.SetUp();
        _sut.ControllerContext.HttpContext = _httpContext.Object;
        _sut.GetDfeClaimsAsync().Wait();
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
        _memoryCache.Dispose();
    }

    [Test]
    public void Given_Accessibility_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController(
            _mockDfeSignInApiService.Object,
            _mockLocalAuthoritySettingsGateway.Object,
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = controller.Accessibility();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("Accessibility");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Privacy_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController(
            _mockDfeSignInApiService.Object,
            _mockLocalAuthoritySettingsGateway.Object,
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = controller.Privacy();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("Privacy");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Cookies_LoadsWithEmptyModel()
    {
        // Arrange
        var controller = new HomeController(
            _mockDfeSignInApiService.Object,
            _mockLocalAuthoritySettingsGateway.Object,
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = controller.Cookies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("Cookies");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_Index_WithValidLocalAuthorityAndRole_ReturnsHomeIndexViewModel()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "FSM - Local Authority Role",
                Code = Constants.RoleCodeLA,
                NumericId = "123"
            }
        };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(roles);

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<HomeIndexViewModel>();

        var model = viewResult.Model as HomeIndexViewModel;
        model.Should().NotBeNull();
        model!.Claims.Roles.Should().HaveCount(1);
        model.Claims.Roles[0].Code.Should().Be(Constants.RoleCodeLA);
        model.SchoolCanReviewEvidence.Should().BeFalse();
    }

    [Test]
    public async Task Given_Index_WithValidSchoolAndRole_ReturnsHomeIndexViewModel_AndFlagTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test School\",\"category\":{{\"id\":1,\"name\":\"{Constants.CategoryTypeSchool}\"}},\"localAuthority\":{{\"code\":\"893\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "FSM - School Role",
                Code = Constants.RoleCodeSchool,
                NumericId = "123"
            }
        };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(roles);

        _mockLocalAuthoritySettingsGateway
            .Setup(g => g.GetLocalAuthoritySettingsAsync(893))
            .ReturnsAsync(new LocalAuthoritySettingsResponse
            {
                SchoolCanReviewEvidence = true
            });

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<HomeIndexViewModel>();

        var model = viewResult.Model as HomeIndexViewModel;
        model.Should().NotBeNull();
        model!.Claims.Roles.Should().HaveCount(1);
        model.Claims.Roles[0].Code.Should().Be(Constants.RoleCodeSchool);
        model.SchoolCanReviewEvidence.Should().BeTrue();
    }

    [Test]
    public async Task Given_Index_WithValidSchoolAndRole_ReturnsHomeIndexViewModel_AndFlagFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test School\",\"category\":{{\"id\":1,\"name\":\"{Constants.CategoryTypeSchool}\"}},\"localAuthority\":{{\"code\":\"893\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "FSM - School Role",
                Code = Constants.RoleCodeSchool,
                NumericId = "123"
            }
        };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(roles);

        _mockLocalAuthoritySettingsGateway
            .Setup(g => g.GetLocalAuthoritySettingsAsync(893))
            .ReturnsAsync(new LocalAuthoritySettingsResponse
            {
                SchoolCanReviewEvidence = false
            });

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<HomeIndexViewModel>();

        var model = viewResult.Model as HomeIndexViewModel;
        model.Should().NotBeNull();
        model!.Claims.Roles.Should().HaveCount(1);
        model.Claims.Roles[0].Code.Should().Be(Constants.RoleCodeSchool);
        model.SchoolCanReviewEvidence.Should().BeFalse();
    }

    [Test]
    public async Task Given_Index_WithValidMATAndRole_ReturnsHomeIndexViewModel()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test MAT\",\"category\":{{\"id\":10,\"name\":\"{Constants.CategoryTypeMAT}\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "FSM - MAT Role",
                Code = Constants.RoleCodeMAT,
                NumericId = "123"
            }
        };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(roles);

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<HomeIndexViewModel>();

        var model = viewResult.Model as HomeIndexViewModel;
        model.Should().NotBeNull();
        model!.Claims.Roles.Should().HaveCount(1);
        model.Claims.Roles[0].Code.Should().Be(Constants.RoleCodeMAT);
        model.SchoolCanReviewEvidence.Should().BeFalse();
    }

    [Test]
    public async Task Given_Index_WithUnknownOrganisationType_ReturnsUnauthorizedOrganizationView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test Unknown\",\"category\":{{\"id\":99,\"name\":\"Unknown Type\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("UnauthorizedOrganization");
    }

    [Test]
    public async Task Given_Index_WithLocalAuthorityButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(new List<Role>());

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithLocalAuthorityAndWrongRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var roles = new List<Role>
        {
            new Role
            {
                Id = Guid.NewGuid(),
                Name = "FSM - School Role",
                Code = Constants.RoleCodeSchool,
                NumericId = "456"
            }
        };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(roles);

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithSchoolButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test School\",\"category\":{{\"id\":1,\"name\":\"{Constants.CategoryTypeSchool}\"}},\"localAuthority\":{{\"code\":\"893\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(new List<Role>());

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithMATButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson =
            $"{{\"id\":\"{orgId}\",\"name\":\"Test MAT\",\"category\":{{\"id\":10,\"name\":\"{Constants.CategoryTypeMAT}\"}}}}";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        _mockDfeSignInApiService
            .Setup(s => s.GetUserRolesAsync(userId, orgId))
            .ReturnsAsync(new List<Role>());

        await _sut.GetDfeClaimsAsync();

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("UnauthorizedRole");
    }
}