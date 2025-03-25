using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Tests.UseCases;

[TestFixture]
public class RegistrationResponseUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<RegistrationUseCase>>();
        _sut = new RegistrationUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private RegistrationUseCase _sut;
    private Mock<ILogger<RegistrationUseCase>> _loggerMock;
    private Fixture _fixture;

    [Test]
    public async Task Execute_WithValidRequest_ReturnsExpectedViewModel()
    {
        // Arrange
        var request = new FsmApplication
        {
            ParentFirstName = "John",
            ParentLastName = "Doe",
            Children = new Children
            {
                ChildList = new List<Child>
                {
                    new()
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        ChildIndex = 1
                    }
                }
            }
        };

        var requestJson = JsonConvert.SerializeObject(request);

        // Act
        var result = await _sut.Execute(requestJson);

        // Assert
        result.Should().NotBeNull();
        result.ParentName.Should().Be("John Doe");
        result.Children.Should().HaveCount(1);
        result.Children[0].ParentName.Should().Be("John Doe");
        result.Children[0].ChildName.Should().Be("Jane Doe");
        result.Children[0].Reference.Should().NotBeNullOrEmpty();
        result.Children[0].Reference.Should().EndWith("-1");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) => v.ToString().Contains("Created registration response for parent John Doe")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Execute_WithMultipleChildren_ReturnsViewModelWithAllChildren()
    {
        // Arrange
        var request = new FsmApplication
        {
            ParentFirstName = "John",
            ParentLastName = "Doe",
            Children = new Children
            {
                ChildList = new List<Child>
                {
                    new() { FirstName = "Jane", LastName = "Doe", ChildIndex = 1 },
                    new() { FirstName = "Jim", LastName = "Doe", ChildIndex = 2 }
                }
            }
        };

        var requestJson = JsonConvert.SerializeObject(request);

        // Act
        var result = await _sut.Execute(requestJson);

        // Assert
        result.Should().NotBeNull();
        result.Children.Should().HaveCount(2);
        result.Children[0].ChildName.Should().Be("Jane Doe");
        result.Children[1].ChildName.Should().Be("Jim Doe");
        result.Children[0].Reference.Should().EndWith("-1");
        result.Children[1].Reference.Should().EndWith("-2");
    }

    [Test]
    public async Task Execute_WithNullChildList_HandlesGracefully()
    {
        // Arrange
        var request = new FsmApplication
        {
            ParentFirstName = "John",
            ParentLastName = "Doe",
            Children = new Children { ChildList = null }
        };

        var requestJson = JsonConvert.SerializeObject(request);

        // Act
        var result = await _sut.Execute(requestJson);

        // Assert
        result.Should().NotBeNull();
        result.ParentName.Should().Be("John Doe");
        result.Children.Should().NotBeNull();
        result.Children.Should().BeEmpty();
    }
}