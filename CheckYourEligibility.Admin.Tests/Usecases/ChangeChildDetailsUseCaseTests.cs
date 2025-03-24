using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class ChangeChildDetailsUseCaseTests
    {
        private ChangeChildDetailsUseCase _sut;
        private Mock<ILogger<ChangeChildDetailsUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ChangeChildDetailsUseCase>>();
            _sut = new ChangeChildDetailsUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void Execute_WithValidJson_ReturnsExpectedChildren()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    new Child
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Day = "01",
                        Month = "01",
                        Year = "2020"
                    }
                }
            };

            var fsmApplication = new FsmApplication { Children = children };
            var json = JsonConvert.SerializeObject(fsmApplication);

            // Act
            var result = _sut.Execute(json);

            // Assert
            result.Should().NotBeNull();
            result.ChildList.Should().NotBeNull();
            result.ChildList.Should().HaveCount(1);

            var firstChild = result.ChildList.First();
            firstChild.FirstName.Should().Be("John");
            firstChild.LastName.Should().Be("Doe");
            firstChild.Day.Should().Be("01");
            firstChild.Month.Should().Be("01");
            firstChild.Year.Should().Be("2020");
        }
    }
}