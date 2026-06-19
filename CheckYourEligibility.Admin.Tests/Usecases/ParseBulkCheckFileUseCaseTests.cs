using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Helpers;
using CheckYourEligibility.Admin.Usecases;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;
using static CheckYourEligibility.Admin.Helpers.CsvBulkCheckValidatorHelper;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class ParseBulkCheckFileUseCaseTests
{
    private Mock<IValidator<CheckEligibilityRequestDataBase>> _validatorMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<ICheckGateway> _checkGatewayMock = null!;
    private ParseBulkCheckFileUseCase _useCase = null!;

    #region Setup

    [SetUp]
    public void Setup()
    {
        _validatorMock = new();
        _serviceProviderMock = new();
        _configurationMock = new();
        _checkGatewayMock = new();

        _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("500");

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<CheckEligibilityRequestDataBase>)))
            .Returns(_validatorMock.Object);

        _checkGatewayMock
            .Setup(c => c.GetSchoolsAsync(It.IsAny<int>()))
            .ReturnsAsync(CreateMockSchools());

        _useCase = new ParseBulkCheckFileUseCase(
            _serviceProviderMock.Object,
            _configurationMock.Object,
            _checkGatewayMock.Object);
    }

    #endregion

    #region Valid File Tests

    [Test]
    public async Task Execute_WithValidCsv_ReturnsValidRequests()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,1985-03-15,AB123456C,
Jane,Doe,1990-06-20,CD987654D,");

        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.Errors, Is.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.ValidRequests[0].LastName, Is.EqualTo("Smith"));
            Assert.That(result.ValidRequests[1].LastName, Is.EqualTo("Doe"));
        });
    }

    [Test]
    public async Task Execute_WithDifferentDateFormats_ParsesCorrectly()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,15/03/1985,AB123456C,
Jane,Doe,20/10/1990,CD987654D,");

        Assert.That(result.ValidRequests[0].DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(result.ValidRequests[1].DateOfBirth, Is.EqualTo("1990-10-20"));
    }

    [Test]
    public async Task Execute_WithLowercaseNI_ConvertsToUppercase()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,1985-03-15,ab123456c,");

        Assert.That(result.ValidRequests[0].NationalInsuranceNumber, Is.EqualTo("AB123456C"));
    }

    [Test]
    public async Task Execute_WithWhitespace_TrimsValues()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
  John  ,  Smith  ,  1985-03-15  ,  AB123456C  ,");

        var request = result.ValidRequests[0];

        Assert.That(request.LastName, Is.EqualTo("Smith"));
        Assert.That(request.DateOfBirth, Is.EqualTo("1985-03-15"));
        Assert.That(request.NationalInsuranceNumber, Is.EqualTo("AB123456C"));
    }

    #endregion

    #region Header Tests

    [Test]
    public async Task Execute_WithMissingHeaders_ReturnsError()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent Last Name,Parent Date of Birth
Smith,1985-03-15");

        Assert.That(result.ErrorMessage, Is.Not.Empty);
        Assert.That(result.ValidRequests, Is.Empty);
    }

    [Test]
    public async Task Execute_WithIncorrectHeaders_ReturnsError()
    {
        SetupValidatorValid();

        var result = await Execute(@"
First Name,Last Name,DOB,NI Number
John,Smith,1985-03-15,AB123456C,");

        Assert.That(result.ErrorMessage, Does.Contain("Missing required header"));
    }

    [Test]
    public async Task Execute_WithNoHeaders_ReturnsError()
    {
        SetupValidatorValid();

        var result = await Execute("Smith,1985-03-15,AB123456C");

        Assert.That(result.ErrorMessage, Is.Not.Empty);
    }

    #endregion

    #region Validation Tests

    [Test]
    public async Task Execute_WithInvalidData_ReturnsErrors()
    {
        SetupValidatorFailures(
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.DateOfBirth), ValidationMessages.DOB),
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.NationalInsuranceNumber), ValidationMessages.NI)
        );

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,invalid-date,BADNI,");

        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Execute_WithMixedRows_ReturnsValidAndErrors()
    {
        var valid = new ValidationResult();
        var invalid = new ValidationResult(new[]
        {
            new ValidationFailure(nameof(CheckEligibilityRequestDataBase.DateOfBirth), ValidationMessages.DOB)
        });

        _validatorMock.SetupSequence(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(valid)
            .ReturnsAsync(invalid)
            .ReturnsAsync(valid);

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,1985-03-15,AB123456C,
Jane,Doe,bad-date,BADNI,
Bob,Jones,1988-01-10,EF654321F,");

        Assert.That(result.ValidRequests.Count, Is.EqualTo(2));
        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Execute_WithDuplicateErrors_Deduplicates()
    {
        SetupValidatorFailures(
            new ValidationFailure("LastName", "Last name is required"),
            new ValidationFailure("LastName", "Last name is required")
        );

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,1985-03-15,AB123456C,");

        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    #endregion

    #region Limits & Edge Cases

    [Test]
    public async Task Execute_ExceedingLimit_ReturnsError()
    {
        _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("2");
        // to inject the test row limit
        _useCase = new ParseBulkCheckFileUseCase(_serviceProviderMock.Object, _configurationMock.Object, _checkGatewayMock.Object);
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
John,Smith,1985-03-15,AB123456C,
Jane,Doe,1990-06-20,CD987654D,
Bob,Jones,1988-01-10,EF654321F,");

        Assert.That(result.ErrorMessage, Does.Contain("more than 2 records"));
    }

    [Test]
    public async Task Execute_EmptyFile_ReturnsNoRecords()
    {
        SetupValidatorValid();

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number");

        Assert.That(result.ValidRequests, Is.Empty);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task Execute_EmptyFields_TriggersValidation()
    {
        SetupValidatorFailures(
            new ValidationFailure("LastName", "Last name is required")
        );

        var result = await Execute(@"
Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number
,,,,");

        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    #endregion

    #region Helpers

    private async Task<BulkCheckCsvResult<CheckEligibilityRequestDataBase>> Execute(string csv)
    {
        var stream = CreateStreamFromString(csv);

        return await _useCase.Execute<CheckEligibilityRequestDataBase>(
            stream,
            CsvBulkCheckValidatorHelper.CreateRequestItem,
            BulkCheckUploadConstants.Headers,
            false,
            123456,
            OrganisationCategory.LocalAuthority);
    }

    private void SetupValidatorValid()
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidatorFailures(params ValidationFailure[] failures)
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CheckEligibilityRequestDataBase>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
    }

    private EstablishmentResponse CreateMockSchools() =>
        new()
        {
            Data = new List<EstablishmentResponseItem>
            {
                new() { URN = 123456, Name = "Test Primary School" },
                new() { URN = 654321, Name = "Test2 Primary School" }
            }
        };

    private Stream CreateStreamFromString(string content)
        => new MemoryStream(Encoding.UTF8.GetBytes(content));

    #endregion
}