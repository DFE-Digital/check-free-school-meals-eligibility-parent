using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Tests.Usecases
{
    [TestFixture]
    public class EnterChildDetailsUseCaseTests
    {
        private Mock<ILogger<EnterChildDetailsUseCase>> _loggerMock;
        private EnterChildDetailsUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<EnterChildDetailsUseCase>>();
            _sut = new EnterChildDetailsUseCase(_loggerMock.Object);
        }

        [Test]
        public async Task Execute_WithNoData_ShouldReturnDefaultChildren()
        {
            // Act
            var result = _sut.Execute(null, null);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().Should().NotBeNull();
        }

        [Test]
        public async Task Execute_WithChildAddOrRemove_ShouldReturnDeserializedChildren()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" },
                new Child { FirstName = "Test2", LastName = "Child2" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = _sut.Execute(childListJson, true);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(2);
            result.ChildList.First().FirstName.Should().Be("Test");
            result.ChildList.First().LastName.Should().Be("Child");
        }

        [Test]
        public async Task Execute_WithChildAddOrRemoveFalse_ShouldReturnDefaultChildren()
        {
            // Arrange
            var childList = new List<Child>
            {
                new Child { FirstName = "Test", LastName = "Child" }
            };
            var childListJson = JsonConvert.SerializeObject(childList);

            // Act
            var result = _sut.Execute(childListJson, false);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);
            result.ChildList.First().FirstName.Should().BeNull();
            result.ChildList.First().LastName.Should().BeNull();
        }
    }
}