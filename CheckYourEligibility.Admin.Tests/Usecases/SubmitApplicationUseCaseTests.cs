using AutoFixture;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using CheckYourEligibility.Admin.ViewModels;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class SubmitApplicationUseCaseTests
    {
        private SubmitApplicationUseCase _sut;
        private Mock<ILogger<SubmitApplicationUseCase>> _loggerMock;
        private Mock<IParentGateway> _parentGatewayMock;
        private IFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<SubmitApplicationUseCase>>();
            _parentGatewayMock = new Mock<IParentGateway>();
            _sut = new SubmitApplicationUseCase(_loggerMock.Object, _parentGatewayMock.Object);

            _fixture = new Fixture();

            // Configure the fixture to omit auto-properties
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private FsmApplication CreateTestApplication()
        {
            return new FsmApplication
            {
                ParentFirstName = "Test",
                ParentLastName = "Parent",
                ParentDateOfBirth = "1990-01-01",
                ParentEmail = "test@example.com",
                ParentNino = "AB123456C",
                Children = new Children
                {
                    ChildList = new List<CheckYourEligibility.Admin.Models.Child>
                    {
                        new CheckYourEligibility.Admin.Models.Child
                        {
                            FirstName = "Test",
                            LastName = "Child",
                            Day = "01",
                            Month = "01",
                            Year = "2020"
                        }
                    }
                }
            };
        }

        private ApplicationSaveItemResponse CreateTestResponse()
        {
            return new ApplicationSaveItemResponse
            {
                Data = new ApplicationResponse
                {
                    ChildFirstName = "Test",
                    ChildLastName = "Child",
                    Reference = "TEST-REF-001",
                    Status = "Entitled"
                }
            };
        }

        [Test]
        public async Task Execute_Should_Submit_Application_For_Each_Child()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "12345"; // Valid numeric establishment ID
            var response = CreateTestResponse();

            _parentGatewayMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.Execute(request, userId, establishment);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(request.Children.ChildList.Count);

            _parentGatewayMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Exactly(request.Children.ChildList.Count));
        }

        [Test]
        public void Execute_Should_Handle_Error_When_Service_Throws()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "12345"; // Valid numeric establishment ID

            _parentGatewayMock
                .Setup(x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
                .Callback<ApplicationRequest>(r => throw new Exception("Service error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _sut.Execute(request, userId, establishment));

            exception.Message.Should().Be("Service error");

            _parentGatewayMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Once);
        }

        [Test]
        public void Execute_Should_Throw_FormatException_When_Establishment_Is_Not_Numeric()
        {
            // Arrange
            var request = CreateTestApplication();
            var userId = "test-user";
            var establishment = "invalid-id";

            
            var exception = Assert.ThrowsAsync<FormatException>(async () =>
                await _sut.Execute(request, userId, establishment));

            
            exception.Message.Should().Contain("format");

            
            _parentGatewayMock.Verify(
                x => x.PostApplication_Fsm(It.IsAny<ApplicationRequest>()),
                Times.Never);
        }
    }
}