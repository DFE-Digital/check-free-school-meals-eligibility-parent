using AutoFixture;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class ValidateParentDetailsUseCaseTests
    {
        private Mock<ILogger<ValidateParentDetailsUseCase>> _loggerMock;
        private ValidateParentDetailsUseCase _sut;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ValidateParentDetailsUseCase>>();
            _sut = new ValidateParentDetailsUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void Execute_WhenNoSelectionAndModelStateValid_ShouldReturnValidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.None;
            var modelState = new ModelStateDictionary();

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeNull();
        }

        [Test]
        public void Execute_WhenNoSelectionAndModelStateInvalid_ShouldReturnInvalidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.None;
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
        public void Execute_WhenAsrnSelectedAndModelStateValid_ShouldReturnValidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
            var modelState = new ModelStateDictionary();

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeNull();
        }

        [Test]
        public void Execute_WhenAsrnSelectedAndModelStateInvalid_ShouldReturnInvalidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("NationalAsylumSeekerServiceNumber", "Required");

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().ContainKey("NationalAsylumSeekerServiceNumber");
        }

        [Test]
        public void Execute_WhenNinSelectedAndModelStateValid_ShouldReturnValidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.NinSelected;
            var modelState = new ModelStateDictionary();

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeNull();
        }

        [Test]
        public void Execute_WhenNinSelectedAndModelStateInvalid_ShouldReturnInvalidResult()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.NinSelected;
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("NationalInsuranceNumber", "Required");

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().ContainKey("NationalInsuranceNumber");
        }

        [Test]
        public void Execute_WhenBothNinAndAsrnErrorsPresent_ShouldConsolidateToNinasError()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.None;
            var modelState = new ModelStateDictionary();

            // Add the specific "Please select one option" error to both fields
            modelState.AddModelError("NationalInsuranceNumber", "Please select one option");
            modelState.AddModelError("NationalAsylumSeekerServiceNumber", "Please select one option");

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().NotContainKey("NationalInsuranceNumber");
            result.Errors.Should().NotContainKey("NationalAsylumSeekerServiceNumber");
            result.Errors.Should().ContainKey("NINAS");
            result.Errors["NINAS"].Should().Contain("Please select one option");
        }

        [Test]
        public void Execute_WhenDifferentErrorMessages_ShouldNotConsolidateToNinasError()
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NinAsrSelection = ParentGuardian.NinAsrSelect.None;
            var modelState = new ModelStateDictionary();

            // Add different error messages
            modelState.AddModelError("NationalInsuranceNumber", "Invalid format");
            modelState.AddModelError("NationalAsylumSeekerServiceNumber", "Please select one option");

            // Act
            var result = _sut.Execute(request, modelState);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.Should().ContainKey("NationalInsuranceNumber");
            result.Errors.Should().ContainKey("NationalAsylumSeekerServiceNumber");
            result.Errors.Should().NotContainKey("NINAS");
        }
    }
}