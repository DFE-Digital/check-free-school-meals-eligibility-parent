using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Tests.Attributes;

[TestFixture]
public class NinoValidatorAttributeTests
{
    #region Test Models

    private class TestModel
    {
        public string Selection { get; set; } = nameof(NinAsrSelect.None);

        [NinValidator("Selection")]
        public string? NationalInsuranceNumber { get; set; }
    }

    #endregion

    #region Valid NIN Format Tests

    [Test]
    [TestCase("AB123456C")]
    [TestCase("CD987654D")]
    [TestCase("AB123456")] // Without suffix
    [TestCase("ZY123456A")]
    [TestCase("ab123456c")] // Lowercase (should normalize)
    [TestCase("AB 12 34 56 C")] // With spaces (should normalize)
    public void IsValid_WithValidNinFormat_ReturnsSuccess(string nin)
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = nin
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    #endregion

    #region Invalid NIN Format Tests

    [Test]
    [TestCase("1234567890")] // All digits
    [TestCase("ABCDEFGHIJ")] // All letters
    [TestCase("AB12345")] // Too short
    [TestCase("AB1234567890")] // Too long (>9)
    [TestCase("AB123456E")] // Invalid suffix (E not allowed)
    [TestCase("AB-123456C")] // Invalid character after normalization
    [TestCase("")] // Empty
    public void IsValid_WithInvalidNinFormat_ReturnsError(string nin)
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = nin
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public void IsValid_WithNinTooLong_ReturnsSpecificError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "AB123456789012" // More than 9 characters
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Does.Contain("no more than 9 alphanumeric characters"));
    }

    #endregion

    #region Selection Logic Tests

    [Test]
    public void IsValid_WhenAsrnSelected_ReturnsSuccess()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.AsrnSelected),
            NationalInsuranceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WhenNoneSelected_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.None),
            NationalInsuranceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Is.EqualTo("Please select one option"));
    }

    [Test]
    public void IsValid_WhenNinSelectedButNull_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Is.EqualTo("National Insurance number is required"));
    }

    #endregion

    #region Normalization Tests

    [Test]
    public void IsValid_WithSpacesInNin_NormalizesAndValidates()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "AB 12 34 56 C"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithLowercaseNin_NormalizesAndValidates()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "ab123456c"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithMixedCaseAndSpaces_NormalizesAndValidates()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "Ab 12 34 56 c"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    #endregion

    #region Valid Suffix Tests

    [Test]
    [TestCase("AB123456A")]
    [TestCase("AB123456B")]
    [TestCase("AB123456C")]
    [TestCase("AB123456D")]
    public void IsValid_WithValidSuffix_ReturnsSuccess(string nin)
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = nin
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithNoSuffix_ReturnsSuccess()
    {
        // Arrange - 8 characters without suffix
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "AB123456"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.NationalInsuranceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Constructor_WithNullSelectionPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NinValidatorAttribute(null!));
    }

    [Test]
    public void IsValid_WithInvalidSelectionProperty_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = nameof(NinAsrSelect.NinSelected),
            NationalInsuranceNumber = "AB123456C"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.NationalInsuranceNumber) };
        var attribute = new NinValidatorAttribute("NonExistentProperty");

        // Act
        var result = attribute.GetValidationResult(model.NationalInsuranceNumber, context);

        // Assert
        Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
        Assert.That(result?.ErrorMessage, Does.Contain("Unknown property"));
    }

    #endregion
}
