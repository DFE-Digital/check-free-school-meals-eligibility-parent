using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.Tests.Attributes.Derived;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes;

public class NassAttributeTests
{
    private const string NASSMissingErrorMessage = "Asylum support reference number is required";
    private const string NASSFormatErrorMessage = "Asylum support reference number must only include numbers";

    private TestableNassAttribute _nassAttribute { get; set; }
    private ValidationContext _validationContext { get; set; }
    private Parent _parent { get; set; }

    [SetUp]
    public void Setup()
    {
        _parent = new Parent
        {
            IsNassSelected = true,
            IsNinoSelected = false
        };
        _nassAttribute = new TestableNassAttribute();
        _validationContext = new ValidationContext(_parent);
    }

    [TestCase(null, NASSMissingErrorMessage)]
    [TestCase("a12345678", NASSFormatErrorMessage)]
    [TestCase("991312345", NASSFormatErrorMessage)]
    [TestCase("991312345", NASSFormatErrorMessage)]
    public void Given_Nass_When_ContainsInvalidCharactersOrIsNull_Should_ReturnErrorMessage(string? nass,
        string? errorMessage)
    {
        // Act
        var result = _nassAttribute.NassIsValid(nass, _validationContext);

        // Assert
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [TestCase("010112345")]
    [TestCase("991200001")]
    [TestCase("9912000001")]
    public void Given_Nass_When_Valid_Should_ReturnNull(string? nass)
    {
        // Act
        var result = _nassAttribute.NassIsValid(nass, _validationContext);

        // Assert
        result.Should().BeEquivalentTo<ValidationResult>(ValidationResult.Success);
        result.Should().BeEquivalentTo(result);
    }
}