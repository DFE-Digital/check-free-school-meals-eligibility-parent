using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.API.Domain.Validation;
using FluentValidation;

namespace CheckYourEligibility.Admin.Domain.Validation;

public class CheckEligibilityRequestDataValidator : AbstractValidator<IEligibilityServiceType>
{
    public CheckEligibilityRequestDataValidator()
    {
        // FSM MVP
        When(x => x is CheckEligibilityRequestDataBase, () =>
        {
            RuleFor(x => (CheckEligibilityRequestDataBase)x)
                .Cascade(CascadeMode.Stop)

                // Last Name (required + valid)
                .ChildRules(baseValidator =>
                {
                    baseValidator.RuleFor(x => x.LastName)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.LastName)
                        .Must(DataValidation.BeAValidName)
                        .WithMessage(ValidationMessages.LastName);

                    // DOB (required + valid)
                    baseValidator.RuleFor(x => x.DateOfBirth)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.DOB)
                        .Must(DataValidation.BeAValidDate)
                        .WithMessage(ValidationMessages.DOB);

                    // NI (required + valid)
                    baseValidator.RuleFor(x => x.NationalInsuranceNumber)
                        .NotEmpty()
                        .Must(DataValidation.BeAValidNi)
                        .When(x => !string.IsNullOrWhiteSpace(x.NationalInsuranceNumber))
                        .WithMessage(ValidationMessages.NI);
                });
        });

        // FSM Enhanced
        When(x => x is CheckEligibilityRequestData_Enhanced, () =>
        {
            RuleFor(x => (CheckEligibilityRequestData_Enhanced)x)
                .Cascade(CascadeMode.Stop)

                .ChildRules(enhanced =>
                {
                    // First Name (required + valid)
                    enhanced.RuleFor(x => x.FirstName)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.FirstName)
                        .Must(DataValidation.BeAValidName)
                        .WithMessage(ValidationMessages.FirstName);

                    // Child First Name (required + valid)
                    enhanced.RuleFor(x => x.ChildFirstName)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.ChildFirstName)
                        .Must(DataValidation.BeAValidName)
                        .WithMessage(ValidationMessages.ChildFirstName);

                    // Child Last Name (required + valid)
                    enhanced.RuleFor(x => x.ChildLastName)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.ChildLastName)
                        .Must(DataValidation.BeAValidName)
                        .WithMessage(ValidationMessages.ChildLastName);

                    // Child DOB (required + valid)
                    enhanced.RuleFor(x => x.ChildDateOfBirth)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.ChildDOB)
                        .Must(DataValidation.BeAValidDate)
                        .WithMessage(ValidationMessages.ChildDOB);

                    // School URN (required + valid + business rule)
                    enhanced.RuleFor(x => x.ChildSchoolUrn)
                        .Cascade(CascadeMode.Stop)
                        .NotEmpty()
                        .WithMessage(ValidationMessages.ChildSchoolUrn)
                        .Must(x => int.TryParse(x, out var urn) && urn > 0)
                        .WithMessage(ValidationMessages.ChildSchoolUrn)
                        .Custom((urnString, context) =>
                        {
                            if (!int.TryParse(urnString, out var urn))
                                return;

                            if (!context.RootContextData.TryGetValue("validSchoolUrns", out var urnSetObj))
                                return;

                            var urnSet = urnSetObj as HashSet<int>;
                            if (urnSet == null || urnSet.Contains(urn))
                                return;

                            context.RootContextData.TryGetValue("organisationType", out var orgTypeObj);
                            var orgType = orgTypeObj as OrganisationCategory?;

                            var message = orgType switch
                            {
                                OrganisationCategory.LocalAuthority =>
                                    ValidationMessages.InvalidSchoolUrnForLA,

                                OrganisationCategory.MultiAcademyTrust =>
                                    ValidationMessages.InvalidSchoolUrnForMAT,

                                _ => "Invalid school URN for this organisation"
                            };

                            context.AddFailure(nameof(CheckEligibilityRequestData_Enhanced.ChildSchoolUrn), message);
                        });
                });
        });
    }
}
