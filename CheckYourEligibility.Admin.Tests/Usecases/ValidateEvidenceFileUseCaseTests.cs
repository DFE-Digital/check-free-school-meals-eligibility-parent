using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.UseCases;

[TestFixture]
public class ValidateEvidenceFileUseCaseTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ValidateEvidenceFileUseCase>>();
        _sut = new ValidateEvidenceFileUseCase(_loggerMock.Object);
        _fixture = new Fixture();
    }

    private Mock<ILogger<ValidateEvidenceFileUseCase>> _loggerMock;
    private ValidateEvidenceFileUseCase _sut;
    private Fixture _fixture;

    [Test]
    public void Execute_WhenFileTypeIsPdfAndSizeIsValid_ShouldReturnValidResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "good-file.pdf";
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        // Act
        var result = _sut.Execute(fileMock.Object);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Test]
    public void Execute_WhenFileTypeIsTxtAndSizeIsValid_ShouldReturnInvalidResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "bad-file.txt";
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("plain/text");

        // Act
        var result = _sut.Execute(fileMock.Object);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("The selected file must be a");
    }

    [Test]
    public void Execute_WhenFileTypeIsPdfAndSizeIsInvalid_ShouldReturnInvalidResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "bad-file.pdf";
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100000000);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        // Act
        var result = _sut.Execute(fileMock.Object);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("The selected file must be smaller than");
    }

}