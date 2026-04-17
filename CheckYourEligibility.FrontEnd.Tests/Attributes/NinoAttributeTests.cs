using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.Tests.Attributes.Derived;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes;

public class NinoAttributeTests
{
    private const string NINOMissingErrorMessage = "National Insurance number is required";
    private const string NINOFormatErrorMessage = "Enter a National Insurance number in the correct format";

    private TestableNinoAttribute _ninoAttribute { get; set; }
    private ValidationContext _validationContext { get; set; }
    private Parent _parent { get; set; }

    [SetUp]
    public void Setup()
    {
        _parent = new Parent();
        _parent.IsNinoSelected = true;
        _ninoAttribute = new TestableNinoAttribute();
        _validationContext = new ValidationContext(_parent);
    }

    [TestCase(null, NINOMissingErrorMessage)]
    [TestCase("DB123456C", NINOFormatErrorMessage)]
    [TestCase("FB123456C", NINOFormatErrorMessage)]
    [TestCase("IB123456C", NINOFormatErrorMessage)]
    [TestCase("QB123456C", NINOFormatErrorMessage)]
    [TestCase("UB123456C", NINOFormatErrorMessage)]
    [TestCase("VB123456C", NINOFormatErrorMessage)]
    [TestCase("AD123456C", NINOFormatErrorMessage)]
    [TestCase("AF123456C", NINOFormatErrorMessage)]
    [TestCase("AI123456C", NINOFormatErrorMessage)]
    [TestCase("AQ123456C", NINOFormatErrorMessage)]
    [TestCase("AU123456C", NINOFormatErrorMessage)]
    [TestCase("AV123456C", NINOFormatErrorMessage)]
    [TestCase("AO123456C", NINOFormatErrorMessage)]
    [TestCase("BG123456C", NINOFormatErrorMessage)]
    [TestCase("GB123456C", NINOFormatErrorMessage)]
    [TestCase("KN123456C", NINOFormatErrorMessage)]
    [TestCase("NK123456C", NINOFormatErrorMessage)]
    [TestCase("NT123456C", NINOFormatErrorMessage)]
    [TestCase("TN123456C", NINOFormatErrorMessage)]
    [TestCase("ZZ123456C", NINOFormatErrorMessage)]
    [TestCase("ZZ123456C", NINOFormatErrorMessage)]
    [TestCase("AB123456E", NINOFormatErrorMessage)]
    public void Given_Nino_When_Invalid_Should_ReturnErrorMessage(string? nino, string? errorMessage)
    {
        // Act
        var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

        // Assert
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [TestCase("ab123456c")]
    [TestCase("AB123456A")]
    [TestCase("AB123456C")]
    [TestCase("AB123456B")]
    [TestCase("AB123456C")]
    [TestCase("AB123456D")]
    public void Given_Nino_When_Valid_Should_ReturnNull(string? nino)
    {
        // Act
        var result = _ninoAttribute.NinoIsValid(nino, _validationContext);

        // Assert
        result.Should().BeNull(nino);
    }
}