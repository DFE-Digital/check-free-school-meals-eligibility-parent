using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Usecases;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class ParseBulkCheckFileUseCase_FsmBasicTests
{
    private Mock<IValidator<CheckEligibilityRequestData_FsmBasic>> _validatorMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private ParseBulkCheckFileUseCase_FsmBasic _useCase = null!;

    [SetUp]
    public void Setup()
    {
        _validatorMock = new Mock<IValidator<CheckEligibilityRequestData_FsmBasic>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("500");

        _useCase = new ParseBulkCheckFileUseCase_FsmBasic(_validatorMock.Object, _configurationMock.Object);
    }

    #region Valid File Tests

    [Test]
    public async Task Execute_WithValidCsv_ReturnsValidRequests()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,1990-06-20,CD987654D,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.ErrorMessage, Is.Empty);
        
        Assert.That(result.ValidRequests[0].LastName, Is.EqualTo("Smith"));
        Assert.That(result.ValidRequests[0].DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(result.ValidRequests[0].NationalInsuranceNumber, Is.EqualTo("AB123456C"));
        Assert.That(result.ValidRequests[0].Sequence, Is.EqualTo(1));

        Assert.That(result.ValidRequests[1].LastName, Is.EqualTo("Doe"));
        Assert.That(result.ValidRequests[1].DateOfBirth, Is.EqualTo("1990-06-20"));
        Assert.That(result.ValidRequests[1].NationalInsuranceNumber, Is.EqualTo("CD987654D"));
        Assert.That(result.ValidRequests[1].Sequence, Is.EqualTo(2));
    }

    [Test]
    public async Task Execute_WithDifferentDateFormats_ParsesCorrectly()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,15/03/1985,AB123456C,\n" +
                        "Jane,Doe,20/10/1990,CD987654D,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.ValidRequests[0].DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(result.ValidRequests[1].DateOfBirth, Is.EqualTo("1990-10-20"));
    }

    [Test]
    public async Task Execute_WithLowercaseNI_ConvertsToUppercase()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,1985-03-15,ab123456c,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests[0].NationalInsuranceNumber, Is.EqualTo("AB123456C"));
    }

    [Test]
    public async Task Execute_WithWhitespaceInFields_TrimsValues()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "  John  ,  Smith  ,  1985-03-15  ,  AB123456C  ,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests[0].LastName, Is.EqualTo("Smith"));
        Assert.That(result.ValidRequests[0].DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(result.ValidRequests[0].NationalInsuranceNumber, Is.EqualTo("AB123456C"));
    }

    #endregion

    #region Invalid Header Tests

    [Test]
    public async Task Execute_WithMissingHeaders_ReturnsErrorMessage()
    {
        // Arrange
        var csvContent = "Parent Last Name,Parent Date of Birth\n" +
                        "Smith,1985-03-15";

        var stream = CreateStreamFromString(csvContent);

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ErrorMessage, Is.Not.Empty);
        Assert.That(result.ValidRequests, Is.Empty);
    }

    [Test]
    public async Task Execute_WithIncorrectHeaders_ReturnsErrorMessage()
    {
        // Arrange
        var csvContent = "First Name,Last Name,DOB,NI Number,ASR Number\n" +
                        "John,Smith,1985-03-15,AB123456C,";

        var stream = CreateStreamFromString(csvContent);

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ErrorMessage, Does.Contain("Missing required header"));
        Assert.That(result.ValidRequests, Is.Empty);
    }

    [Test]
    public async Task Execute_WithNoHeaders_ReturnsErrorMessage()
    {
        // Arrange
        var csvContent = "Smith,1985-03-15,AB123456C";

        var stream = CreateStreamFromString(csvContent);

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    #endregion

    #region Validation Error Tests

    [Test]
    public async Task Execute_WithInvalidData_ReturnsErrors()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,invalid-date,BADNI,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("DateOfBirth", "Invalid date format"),
            new ValidationFailure("NationalInsuranceNumber", "Invalid NI number")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(result.Errors[0].Message, Is.EqualTo("Invalid date format"));
        Assert.That(result.Errors[1].LineNumber, Is.EqualTo(2));
        Assert.That(result.Errors[1].Message, Is.EqualTo("Invalid NI number"));
    }

    [Test]
    public async Task Execute_WithMixedValidAndInvalidRows_ReturnsBoth()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,bad-date,BADNI,\n" +
                        "Bob,Jones,1988-01-10,EF654321F,";

        var stream = CreateStreamFromString(csvContent);

        var validResult = new ValidationResult();
        var invalidResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("DateOfBirth", "Invalid date format")
        });

        _validatorMock
            .SetupSequence(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(validResult)
            .ReturnsAsync(invalidResult)
            .ReturnsAsync(validResult);

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors[0].LineNumber, Is.EqualTo(3));
    }

    [Test]
    public async Task Execute_WithDuplicateErrorsForSameRow_DoesNotDuplicate()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,1985-03-15,AB123456C,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("LastName", "Last name is required"),
            new ValidationFailure("LastName", "Last name is required") // Duplicate
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    #endregion

    #region Row Limit Tests

    [Test]
    public async Task Execute_ExceedingRowLimit_ReturnsError()
    {
        // Arrange
        _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("2");
        _useCase = new ParseBulkCheckFileUseCase_FsmBasic(_validatorMock.Object, _configurationMock.Object);

        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,1990-06-20,CD987654D,\n" +
                        "Bob,Jones,1988-01-10,EF654321F,"; // This exceeds limit of 2

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ErrorMessage, Does.Contain("cannot contain more than 2 records"));
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task Execute_WithEmptyFile_ReturnsNoRecords()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n";

        var stream = CreateStreamFromString(csvContent);

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.ErrorMessage, Is.Empty);
    }

    [Test]
    public async Task Execute_WithEmptyFields_PassesToValidator()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance Number,Parent Asylum Seeker Reference Number\n" +
                        ",,,,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("LastName", "Last name is required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestData_FsmBasic>(), default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _useCase.Execute(stream);

        // Assert
        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    #endregion

    #region Helper Methods

    private Stream CreateStreamFromString(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    #endregion
}
