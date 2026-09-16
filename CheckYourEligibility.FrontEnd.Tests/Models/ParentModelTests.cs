using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.ViewModels;

public class ParentModelTests
{
    private Parent _parent { get; set; }
    private ValidationContext _validationContext { get; set; }
    private List<ValidationResult> _validationResults { get; set; }

    [SetUp]
    public void Setup()
    {
        _parent = new Parent();
        _validationContext = new ValidationContext(_parent);
        _validationResults = new List<ValidationResult>();
    }

    [TestCase(null, null, true, false, null, null, null, null, null, 4, "Expected 4 validation errors")]
    [TestCase(null, null, false, true, null, null, null, null, null, 4, "Expected 4 validation errors")]
    [TestCase("AB123456C", null, false, null, "Homer", "Simpson", "32", "01", "1990", 1,
        "Expected 1 validation error for invalid day")]
    [TestCase("AB123456C", null, false, null, "Homer", "Simpson", "31", "13", "1990", 1,
        "Expected 1 validation error for invalid month")]
    [TestCase("GB123456A", null, true, null, "Homer", "Simpson", "32", "13", "1990", 2,
        "Expected 2 validation error for nino and date")]
    public void Given_InvalidParentModel_When_Validated_Should_ReturnExpectedNumberOfErrors(string? nino, string? nass,
        bool? isNinoSelected, bool? isNassSelected, string? firstName, string? lastName, string? day, string? month,
        string? year, int numberOfErrors, string reason)
    {
        // Arrange
        _parent.NationalInsuranceNumber = nino;
        _parent.NationalAsylumSeekerServiceNumber = nass;
        _parent.IsNinoSelected = isNinoSelected;
        _parent.IsNassSelected = isNassSelected;
        _parent.FirstName = firstName;
        _parent.LastName = lastName;
        _parent.Day = day;
        _parent.Month = month;
        _parent.Year = year;

        // Act
        var isValid = Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);


        //Assert

        isValid.Should().BeFalse(reason);
        _validationResults.Count.Should().Be(numberOfErrors, reason);


        _validationResults.Count.Should().Be(numberOfErrors);
    }

    [Test]
    public void Given_EnterDetailsPage_When_IsNinoSelectedIsNull_Should_ReturnErrorAgainstIsNinoSelected()
    {
        // Arrange
        _parent.FirstName = "Homer";
        _parent.LastName = "Simpson";
        _parent.Day = "1";
        _parent.Month = "1";
        _parent.Year = "1990";
        _parent.NASSRedirect = false;
        _parent.IsNinoSelected = null;
        _parent.IsNassSelected = null;

        // Act
        Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);

        // Assert
        _validationResults.Should().ContainSingle(r =>
            r.ErrorMessage == "Select yes if you have a National Insurance number");
        _validationResults.Should().NotContain(r =>
            r.ErrorMessage == "Select yes if you have an asylum support reference number");
    }

    [Test]
    public void Given_NassPage_When_IsNassSelectedIsNull_Should_ReturnErrorAgainstIsNassSelected()
    {
        // Arrange
        _parent.FirstName = "Homer";
        _parent.LastName = "Simpson";
        _parent.Day = "1";
        _parent.Month = "1";
        _parent.Year = "1990";
        _parent.NASSRedirect = true;
        _parent.IsNinoSelected = false;
        _parent.IsNassSelected = null;

        // Act
        Validator.TryValidateObject(_parent, _validationContext, _validationResults, true);

        // Assert
        _validationResults.Should().ContainSingle(r =>
            r.ErrorMessage == "Select yes if you have an asylum support reference number");
        _validationResults.Should().NotContain(r =>
            r.ErrorMessage == "Select yes if you have a National Insurance number");
    }
}