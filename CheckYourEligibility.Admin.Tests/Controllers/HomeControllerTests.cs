using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
internal class HomeControllerTests : TestBase
{
    private Mock<IDfeSignInApiService> _mockDfeSignInApiService;
    private HomeController _sut;

    [SetUp]
    public void SetUp()
    {
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        _sut = new HomeController(_mockDfeSignInApiService.Object);
		base.SetUp();
		_sut.ControllerContext.HttpContext = _httpContext.Object;
		_sut.GetDfeClaimsAsync().Wait();
	}

	[TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Given_Accessibility_LoadsWithEmptyModel()
    {
        // Arrange
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        var controller = new HomeController(_mockDfeSignInApiService.Object);

        // Act
        var result = controller.Accessibility();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Accessibility");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Privacy_LoadsWithEmptyModel()
    {
        // Arrange
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        var controller = new HomeController(_mockDfeSignInApiService.Object);

        // Act
        var result = controller.Privacy();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Privacy");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void Given_Cookies_LoadsWithEmptyModel()
    {
        // Arrange
        _mockDfeSignInApiService = new Mock<IDfeSignInApiService>();
        var controller = new HomeController(_mockDfeSignInApiService.Object);

        // Act
        var result = controller.Cookies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Cookies");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_Index_WithValidLocalAuthorityAndRole_ReturnsClaims()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
		var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "FSM - Local Authority Role", Code = Constants.RoleCodeLA, NumericId = "123" }
        };
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(roles);
        await _sut.GetDfeClaimsAsync();

		// Act
		var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<DfeClaims>();
        
        var dfeClaims = viewResult.Model as DfeClaims;
        dfeClaims.Roles.Should().HaveCount(1);
        dfeClaims.Roles[0].Code.Should().Be(Constants.RoleCodeLA);
    }

    [Test]
    public async Task Given_Index_WithValidSchoolAndRole_ReturnsClaims()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test School\",\"category\":{{\"id\":1,\"name\":\"{Constants.CategoryTypeSchool}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
		var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "FSM - School Role", Code = Constants.RoleCodeSchool, NumericId = "123" }
        };
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(roles);
		await _sut.GetDfeClaimsAsync();

		// Act
		var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<DfeClaims>();
        
        var dfeClaims = viewResult.Model as DfeClaims;
        dfeClaims.Roles.Should().HaveCount(1);
        dfeClaims.Roles[0].Code.Should().Be(Constants.RoleCodeSchool);
    }

    [Test]
    public async Task Given_Index_WithValidMATAndRole_ReturnsClaims()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test MAT\",\"category\":{{\"id\":10,\"name\":\"{Constants.CategoryTypeMAT}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

		var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "FSM - MAT Role", Code = Constants.RoleCodeMAT, NumericId = "123" }
        };
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(roles);
		await _sut.GetDfeClaimsAsync();

		// Act
		var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().BeNull();
        viewResult.Model.Should().BeOfType<DfeClaims>();
        
        var dfeClaims = viewResult.Model as DfeClaims;
        dfeClaims.Roles.Should().HaveCount(1);
        dfeClaims.Roles[0].Code.Should().Be(Constants.RoleCodeMAT);
    }

    [Test]
    public async Task Given_Index_WithUnknownOrganisationType_ReturnsUnauthorizedOrganizationView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test Unknown\",\"category\":{{\"id\":99,\"name\":\"Unknown Type\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
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
        viewResult.ViewName.Should().Be("UnauthorizedOrganization");
    }

    [Test]
    public async Task Given_Index_WithLocalAuthorityButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Return no roles
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(new List<Role>());

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithLocalAuthorityAndWrongRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test LA\",\"category\":{{\"id\":2,\"name\":\"{Constants.CategoryTypeLA}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Return a different role (school role for an LA user)
        var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "FSM - School Role", Code = Constants.RoleCodeSchool, NumericId = "456" }
        };
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(roles);

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithSchoolButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test School\",\"category\":{{\"id\":1,\"name\":\"{Constants.CategoryTypeSchool}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Return no roles
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(new List<Role>());

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedRole");
    }

    [Test]
    public async Task Given_Index_WithMATButNoRole_ReturnsUnauthorizedRoleView()
    {
        // Arrange
        var userId = "test-user-id";
        var orgId = Guid.NewGuid();
        var organisationJson = $"{{\"id\":\"{orgId}\",\"name\":\"Test MAT\",\"category\":{{\"id\":10,\"name\":\"{Constants.CategoryTypeMAT}\"}}}}";
        
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User"),
            new Claim("organisation", organisationJson)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Return no roles
        _mockDfeSignInApiService.Setup(s => s.GetUserRolesAsync(userId, orgId)).ReturnsAsync(new List<Role>());

        // Act
        var result = await _sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult.ViewName.Should().Be("UnauthorizedRole");
    }
}