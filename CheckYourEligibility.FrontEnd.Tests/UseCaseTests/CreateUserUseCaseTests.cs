using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.UseCases;
using FluentAssertions;
using Moq;

namespace CheckYourEligibility.FrontEnd.Tests.UseCases;

[TestFixture]
public class CreateUserUseCaseTests
{
    [Test]
    public async Task Execute_WhenCreatingOneLoginUser_ShouldSendNoOrganisationMetadata()
    {
        // Arrange
        var parentGateway = new Mock<IParentGateway>();
        UserCreateRequest? capturedRequest = null;

        parentGateway.Setup(x => x.CreateUser(It.IsAny<UserCreateRequest>()))
            .Callback<UserCreateRequest>(request => capturedRequest = request)
            .ReturnsAsync(new UserSaveItemResponse { Data = "user-id" });

        var sut = new CreateUserUseCase(parentGateway.Object);

        // Act
        var result = await sut.Execute("parent@example.com", "one-login-sub");

        // Assert
        result.Should().Be("user-id");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Data.Should().BeEquivalentTo(new UserData
        {
            Email = "parent@example.com",
            Reference = "one-login-sub"
        });
        capturedRequest.MetaData.Should().BeEquivalentTo(new CheckMetaData
        {
            UserName = "parent@example.com",
            OrganisationID = 0,
            OrganisationType = "none"
        });
    }
}