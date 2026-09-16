using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

internal class IsNassSelectedAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;

        // On the NASS page: user already said "No" to having a NINO, so they must
        // select whether they have an asylum support reference number.
        if (model.NASSRedirect && model.IsNassSelected == null)
            return new ValidationResult("Select yes if you have an asylum support reference number");

        return ValidationResult.Success;
    }
}
