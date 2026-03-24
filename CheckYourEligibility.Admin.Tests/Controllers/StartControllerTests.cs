using CheckYourEligibility.Admin.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
internal class StartControllerTests
{
    private static StartController CreateController(bool isAuthenticated, bool redirectSetting)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RedirectAuthenticatedUsersFromStartPage"] = redirectSetting.ToString()
            })
            .Build();

        var userMock = new Mock<System.Security.Claims.ClaimsPrincipal>();
        userMock.Setup(u => u.Identity!.IsAuthenticated).Returns(isAuthenticated);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(ctx => ctx.User).Returns(userMock.Object);

        var controller = new StartController(config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        return controller;
    }

    [Test]
    public void Index_WhenUserIsNotAuthenticated_ReturnsStartView()
    {
        // Arrange
        var sut = CreateController(isAuthenticated: false, redirectSetting: true);

        // Act
        var result = sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
    }

    [Test]
    public void Index_WhenUserIsAuthenticated_AndRedirectSettingIsTrue_RedirectsToDashboard()
    {
        // Arrange
        var sut = CreateController(isAuthenticated: true, redirectSetting: true);

        // Act
        var result = sut.Index();

        // Assert
        var redirect = result as RedirectResult;
        redirect.Should().NotBeNull();
        redirect!.Url.Should().Be("/home");
    }

    [Test]
    public void Index_WhenUserIsAuthenticated_AndRedirectSettingIsFalse_ReturnsStartView()
    {
        // Arrange
        var sut = CreateController(isAuthenticated: true, redirectSetting: false);

        // Act
        var result = sut.Index();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
    }

    [Test]
    public void Given_Privacy_LoadsWithEmptyModel()
    {
        // Arrange
        var sut = CreateController(isAuthenticated: false, redirectSetting: true);


        // Act
        var result = sut.Privacy();

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
        var sut = CreateController(isAuthenticated: false, redirectSetting: true);

        // Act
        var result = sut.Cookies();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("Cookies");
        viewResult.Model.Should().BeNull();
    }
}
