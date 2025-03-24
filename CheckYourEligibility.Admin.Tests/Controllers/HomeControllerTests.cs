using CheckYourEligibility.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace CheckYourEligibility.Admin.Tests.Controllers
{
    [TestFixture]
    internal class HomeControllerTests
    {

        private HomeController _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new HomeController();
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
            var controller = new HomeController();

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
            var controller = new HomeController();

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
            var controller = new HomeController();

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Cookies");
            viewResult.Model.Should().BeNull();
        }
    }
}
