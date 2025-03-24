using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class InitializeCheckAnswersUseCaseTests
    {
        private InitializeCheckAnswersUseCase _sut;
        private Mock<ILogger<InitializeCheckAnswersUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<InitializeCheckAnswersUseCase>>();
            _sut = new InitializeCheckAnswersUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_WithValidJson_ReturnsFsmApplication()
        {
            // Arrange
            var application = _fixture.Create<FsmApplication>();
            var json = JsonConvert.SerializeObject(application);

            // Act
            var result = await _sut.Execute(json);

            // Assert
            result.Should().BeEquivalentTo(application);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully initialized")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WithNullJson_ReturnsNull()
        {
            // Act
            var result = await _sut.Execute(null);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No FSM application data found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Execute_WithInvalidJson_ReturnsNull()
        {
            // Arrange
            const string invalidJson = "invalid json string";

            // Act
            var result = await _sut.Execute(invalidJson);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialize")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void Execute_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Throws(new Exception("Test exception"));

            // Act & Assert
            _sut.Invoking(x => x.Execute("any string"))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Test exception");
        }
    }
}