using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Tests.Attributes;

[TestFixture]
public class NassAttributeTests
{
    #region Test Models

    private class TestModel
    {
        public string Selection { get; set; } = "None";

        [Nass("Selection")]
        public string? AsylumSeekerReferenceNumber { get; set; }
    }

    private class TestModelCustomNames
    {
        public string IdType { get; set; } = "NotSelected";

        [Nass("IdType", "NotSelected", "NationalInsurance", "AsylumReference")]
        public string? AsrNumber { get; set; }
    }

    #endregion

    #region Valid NASS Format Tests

    [Test]
    [TestCase("0101123456")]
    [TestCase("1212987654")]
    [TestCase("0612345678")]
    [TestCase("12011234567")] // 6 digit variant
    public void IsValid_WithValidNassFormat_ReturnsSuccess(string nass)
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "AsrnSelected",
            AsylumSeekerReferenceNumber = nass
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var attribute = new NassAttribute("Selection");
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    #endregion

    #region Invalid NASS Format Tests

    [Test]
    [TestCase("0001123456")] // Invalid month (00)
    [TestCase("0113123456")] // Invalid month (13)
    [TestCase("AB01123456")] // Contains letters
    [TestCase("010112345")] // Too short
    [TestCase("01011234567890")] // Too long
    [TestCase("01-01-12345")] // Contains hyphens
    [TestCase("")] // Empty
    public void IsValid_WithInvalidNassFormat_ReturnsError(string nass)
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "AsrnSelected",
            AsylumSeekerReferenceNumber = nass
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results, Is.Not.Empty);
    }

    #endregion

    #region Selection Logic Tests

    [Test]
    public void IsValid_WhenNinSelected_ReturnsSuccess()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "NinSelected",
            AsylumSeekerReferenceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WhenNoneSelected_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "None",
            AsylumSeekerReferenceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Is.EqualTo("Please select one option"));
    }

    [Test]
    public void IsValid_WhenAsrnSelectedButEmpty_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "AsrnSelected",
            AsylumSeekerReferenceNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Is.EqualTo("Asylum support reference number is required"));
    }

    [Test]
    public void IsValid_WhenAsrnSelectedButWhitespace_ReturnsError()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "AsrnSelected",
            AsylumSeekerReferenceNumber = "   "
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
    }

    #endregion

    #region Custom Names Tests

    [Test]
    public void IsValid_WithCustomNames_WhenNationalInsuranceSelected_ReturnsSuccess()
    {
        // Arrange
        var model = new TestModelCustomNames
        {
            IdType = "NationalInsurance",
            AsrNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModelCustomNames.AsrNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsrNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithCustomNames_WhenAsylumReferenceSelectedAndValid_ReturnsSuccess()
    {
        // Arrange
        var model = new TestModelCustomNames
        {
            IdType = "AsylumReference",
            AsrNumber = "0101123456"
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModelCustomNames.AsrNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsrNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithCustomNames_WhenNotSelectedAndEmpty_ReturnsError()
    {
        // Arrange
        var model = new TestModelCustomNames
        {
            IdType = "NotSelected",
            AsrNumber = null
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModelCustomNames.AsrNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsrNumber, context, results);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results[0].ErrorMessage, Is.EqualTo("Please select one option"));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void IsValid_WithLeadingAndTrailingSpaces_ValidatesCorrectly()
    {
        // Arrange
        var model = new TestModel
        {
            Selection = "AsrnSelected",
            AsylumSeekerReferenceNumber = "  0101123456  "
        };

        var context = new ValidationContext(model) { MemberName = nameof(TestModel.AsylumSeekerReferenceNumber) };
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateProperty(model.AsylumSeekerReferenceNumber, context, results);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Constructor_WithNullSelectionPropertyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NassAttribute(null!));
    }

    #endregion
}
