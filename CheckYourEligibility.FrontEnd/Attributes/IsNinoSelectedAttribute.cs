using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

internal class IsNinoSelectedAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;

        // On the Enter_Details page: user must select whether they have a NINO.
        if (model.IsNinoSelected == null)
            return new ValidationResult("Select yes if you have a National Insurance number");

        return ValidationResult.Success;
    }
}