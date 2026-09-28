using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Attributes;
using CheckYourEligibility.FrontEnd.Models;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Attributes;

[TestFixture]
public class NameAttributeRegressionTests
{
    [Test]
    public void Validate_IdenticalInvalidNames_UsesLastNameMessage()
    {
        var model = new Parent
        {
            FirstName = "Smith@",
            LastName = "Smith@"
        };

        var attribute = new NameAttribute();
        var context = new ValidationContext(model)
        {
            MemberName = nameof(Parent.LastName)
        };

        var result = attribute.GetValidationResult(model.LastName, context);

        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Be("Enter a last name with valid characters");
    }
}
