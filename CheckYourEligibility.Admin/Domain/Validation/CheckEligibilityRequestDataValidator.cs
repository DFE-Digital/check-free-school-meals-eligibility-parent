// Ignore Spelling: Validator

using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckYourEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator : AbstractValidator<IEligibilityServiceType>
{
    public CheckEligibilityRequestDataValidator()
    {
          // Rules for FSM MVP
        When(x => x is CheckEligibilityRequestDataBase, () =>
        {
            When(x => string.IsNullOrEmpty(((CheckEligibilityRequestDataBase)x).LastName), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestDataBase)x).LastName)
                    .NotEmpty()
                    .WithMessage(ValidationMessages.LastName);
            });
            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestDataBase)x).LastName), () => {
                RuleFor(x => ((CheckEligibilityRequestDataBase)x).LastName)
                    .Must(DataValidation.BeAValidName)
                    .WithMessage(ValidationMessages.LastName);
            });

            RuleFor(x => ((CheckEligibilityRequestDataBase)x).DateOfBirth)
                .NotEmpty()
                .Must(DataValidation.BeAValidDate)
                .WithMessage(ValidationMessages.DOB);

            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestDataBase)x).NationalInsuranceNumber), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestDataBase)x).NationalInsuranceNumber)
                    .Must(DataValidation.BeAValidNi)
                    .WithMessage(ValidationMessages.NI);
            });
        });
        // Rules for FSM Enhanced
        When(x => x is CheckEligibilityRequestData_Enhanced, () =>
        {

            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData_Enhanced)x).FirstName), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestData_Enhanced)x).FirstName)
                    .Must(DataValidation.BeAValidName)
                    .WithMessage(ValidationMessages.FirstName);
            });
            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData_Enhanced)x).ChildFirstName), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestData_Enhanced)x).ChildFirstName)
                    .Must(DataValidation.BeAValidName)
                    .WithMessage(ValidationMessages.ChildFirstName);
            });

            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData_Enhanced)x).ChildLastName), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestData_Enhanced)x).ChildLastName)
                    .Must(DataValidation.BeAValidName)
                    .WithMessage(ValidationMessages.ChildLastName);
            });

            When(x => !string.IsNullOrEmpty(((CheckEligibilityRequestData_Enhanced)x).ChildDateOfBirth), () =>
            {
                RuleFor(x => ((CheckEligibilityRequestData_Enhanced)x).ChildDateOfBirth)
                    .Must(DataValidation.BeAValidDate)
                    .WithMessage(ValidationMessages.ChildDOB);
            });
        });

        When((x, context) => x is CheckEligibilityRequestData_Enhanced e && 
        context.RootContextData.TryGetValue("isEnhancedSchool", out var value) && value is bool isEnhancedSchool && !isEnhancedSchool, () =>
        {           
                RuleFor(x => ((CheckEligibilityRequestData_Enhanced)x).ChildSchoolUrn)
                    .Must(x => int.TryParse(x, out var urn) && urn > 0)
                    .WithMessage(ValidationMessages.ChildSchoolUrn);
          
        });
    }
}