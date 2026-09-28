using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Domain.Validation;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Validation;

[TestFixture]
public class ApplicationRequestValidatorDateTests
{
    [Test]
    public void Validate_ImpossibleParentDateOfBirth_ReturnsValidationError()
    {
        var request = new ApplicationRequest
        {
            Data = new ApplicationRequestData
            {
                ParentFirstName = "Homer",
                ParentLastName = "Simpson",
                ParentEmail = "homer@example.com",
                ParentNationalInsuranceNumber = "AB123456C",
                ParentDateOfBirth = "2026-02-31",
                ChildFirstName = "Bart",
                ChildLastName = "Simpson",
                ChildDateOfBirth = "2015-01-01"
            }
        };

        var validator = new ApplicationRequestValidator();

        var result = validator.Validate(request);

        result.Errors.Should().Contain(
            x => x.PropertyName.EndsWith(nameof(ApplicationRequestData.ParentDateOfBirth)));
    }
}
