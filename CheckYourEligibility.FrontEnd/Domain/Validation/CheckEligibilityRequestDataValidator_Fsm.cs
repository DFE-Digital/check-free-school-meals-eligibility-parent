// Ignore Spelling: Validator

using CheckYourEligibility.API.Domain.Validation;
using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Domain.Constants.ErrorMessages;
using FluentValidation;

namespace CheckYourEligibility.FrontEnd.Domain.Validation;

public class CheckEligibilityRequestDataValidator_Fsm : AbstractValidator<CheckEligibilityRequestData_Fsm>
{
    public CheckEligibilityRequestDataValidator_Fsm()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessages.LastName);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(DataValidation.BeAValidDate)
            .WithMessage(ValidationMessages.DOB);

        When(x => !string.IsNullOrEmpty(x.NationalInsuranceNumber), () =>
        {
            RuleFor(x => x.NationalAsylumSeekerServiceNumber)
                .Empty()
                .WithMessage(ValidationMessages.NI_and_NASS);
            RuleFor(x => x.NationalInsuranceNumber)
                .NotEmpty()
                .Must(DataValidation.BeAValidNi)
                .WithMessage(ValidationMessages.NI);
        }).Otherwise(() =>
        {
            RuleFor(x => x.NationalAsylumSeekerServiceNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidationMessages.NI_or_NASS)
                .Must(DataValidation.BeAValidNass)
                .WithMessage(ValidationMessages.NI_or_NASS);
        });
    }
}