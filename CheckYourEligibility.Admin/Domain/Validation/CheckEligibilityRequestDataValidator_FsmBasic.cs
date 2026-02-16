// Ignore Spelling: Validator

using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckYourEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator_FsmBasic : AbstractValidator<CheckEligibilityRequestData_FsmBasic>
{
    public CheckEligibilityRequestDataValidator_FsmBasic()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessages.LastName);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(DataValidation.BeAValidDate)
            .WithMessage(ValidationMessages.DOB);

        RuleFor(x => x.NationalInsuranceNumber)
            .NotEmpty()
            .Must(DataValidation.BeAValidNi)
            .WithMessage(ValidationMessages.NI);
    }
}
