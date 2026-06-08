using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateParentDetailsUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidateParentDetailsUseCase>>();
        _sut = new ValidateParentDetailsUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private Mock<ILogger<ValidateParentDetailsUseCase>> _loggerMock;
    private ValidateParentDetailsUseCase _sut;
    private Fixture _fixture;

    [Test]
    public void Execute_WhenModelStateValid_ShouldReturnValidResult()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();

        // Act
        var result = _sut.Execute(request, modelState);

        // Assert
        result.IsValid.Should().BeTrue();
        (result.Errors == null || result.Errors.Count == 0).Should().BeTrue();
    }

    [Test]
    public void Execute_WhenNoSelectionAndModelStateInvalid_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("TestKey", "Test Error");

        // Act
        var result = _sut.Execute(request, modelState);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("TestKey");
    }

    [Test]
    public void Execute_WhenNinSelectedAndModelStateInvalid_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("NationalInsuranceNumber", "Required");

        // Act
        var result = _sut.Execute(request, modelState);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().ContainKey("NationalInsuranceNumber");
    }
}