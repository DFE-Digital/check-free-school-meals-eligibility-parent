using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.Admin.Helpers;
using CheckYourEligibility.Admin.Usecases;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class ParseBulkCheckFileUseCaseTests
{
    private Mock<IValidator<CheckEligibilityRequestDataBase>> _validatorMock = null!;
    private Mock<IServiceProvider> _serviceProvider = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private ParseBulkCheckFileUseCase _useCase = null!;

    [SetUp]
    public void Setup()
    {
        _validatorMock = new Mock<IValidator<CheckEligibilityRequestDataBase>>();
        _serviceProvider = new Mock<IServiceProvider>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("500");

        _useCase = new ParseBulkCheckFileUseCase(_serviceProvider.Object, _configurationMock.Object);
    }

    #region Valid File Tests

    [Test]
    public async Task Execute_WithValidCsv_ReturnsValidRequests()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,1990-06-20,CD987654D,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _serviceProvider
         .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
         .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(
            stream,
            CsvBulkCheckValidatorHelper.CreateRequestItem,
            BulkCheckUploadConstants.Headers,
            false);

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
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "John,Smith,15/03/1985,AB123456C,\n" +
                        "Jane,Doe,20/10/1990,CD987654D,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.ValidRequests[0].DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(result.ValidRequests[1].DateOfBirth, Is.EqualTo("1990-10-20"));
    }

    [Test]
    public async Task Execute_WithLowercaseNI_ConvertsToUppercase()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "John,Smith,1985-03-15,ab123456c,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
       .Setup(v => v.ValidateAsync(
           It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
           It.IsAny<CancellationToken>()))
       .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ValidRequests[0].NationalInsuranceNumber, Is.EqualTo("AB123456C"));
    }

    [Test]
    public async Task Execute_WithWhitespaceInFields_TrimsValues()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "  John  ,  Smith  ,  1985-03-15  ,  AB123456C  ,";

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
     .Setup(v => v.ValidateAsync(
         It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
         It.IsAny<CancellationToken>()))
     .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

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

        _validatorMock
    .Setup(v => v.ValidateAsync(
        It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

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

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

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

        _validatorMock
        .Setup(v => v.ValidateAsync(
            It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    #endregion

    #region Validation Error Tests

    [Test]
    public async Task Execute_WithInvalidData_ReturnsErrors()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number\n" +
                        "John,Smith,invalid-date,BADNI,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.DateOfBirth), ValidationMessages.DOB),
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.NationalInsuranceNumber), ValidationMessages.NI)
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
        Assert.That(result.Errors[0].LineNumber, Is.EqualTo(2));
        Assert.That(result.Errors[0].Message, Is.EqualTo(ValidationMessages.DOB));
        Assert.That(result.Errors[1].LineNumber, Is.EqualTo(2));
        Assert.That(result.Errors[1].Message, Is.EqualTo(ValidationMessages.NI));
    }

    [Test]
    public async Task Execute_WithMixedValidAndInvalidRows_ReturnsBoth()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,bad-date,BADNI,\n" +
                        "Bob,Jones,1988-01-10,EF654321F,";

        var stream = CreateStreamFromString(csvContent);

        var validResult = new ValidationResult();
        var invalidResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.DateOfBirth), ValidationMessages.DOB)
        });

        _validatorMock
            .SetupSequence(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult)
            .ReturnsAsync(invalidResult)
            .ReturnsAsync(validResult);

        _serviceProvider
          .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
          .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(result.Errors[0].LineNumber, Is.EqualTo(3));
    }

    [Test]
    public async Task Execute_WithDuplicateErrorsForSameRow_DoesNotDuplicate()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "John,Smith,1985-03-15,AB123456C,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("LastName", "Last name is required"),
            new ValidationFailure("LastName", "Last name is required") // Duplicate
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestDataBase>(), default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        _serviceProvider
          .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
          .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

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
        _useCase = new ParseBulkCheckFileUseCase(_serviceProvider.Object, _configurationMock.Object);

        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        "John,Smith,1985-03-15,AB123456C,\n" +
                        "Jane,Doe,1990-06-20,CD987654D,\n" +
                        "Bob,Jones,1988-01-10,EF654321F,"; // This exceeds limit of 2

        var stream = CreateStreamFromString(csvContent);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestDataBase>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _serviceProvider
          .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
          .Returns(_validatorMock.Object);

        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ErrorMessage, Does.Contain("CSV file cannot contain more than 2 records"));
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task Execute_WithEmptyFile_ReturnsNoRecords()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n";

        var stream = CreateStreamFromString(csvContent);
        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _serviceProvider
          .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
          .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

        // Assert
        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors, Is.Empty);
        Assert.That(result.ErrorMessage, Is.Empty);
    }

    [Test]
    public async Task Execute_WithEmptyFields_PassesToValidator()
    {
        // Arrange
        var csvContent = "Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number\n" +
                        ",,,,";

        var stream = CreateStreamFromString(csvContent);

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("LastName", "Last name is required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CheckEligibilityRequestDataBase>(), default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        _serviceProvider
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);
        // Act
        var result = await _useCase.Execute<CheckEligibilityRequestDataBase>(stream, CsvBulkCheckValidatorHelper.CreateRequestItem, BulkCheckUploadConstants.Headers, false);

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
