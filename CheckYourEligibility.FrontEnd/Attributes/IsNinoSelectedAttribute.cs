using System.ComponentModel.DataAnnotations;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

internal class IsNinoSelectedAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;
        if (model.IsNassSelected == null && model.IsNinoSelected == null && model.NASSRedirect == false)
            return new ValidationResult("Select yes if you have a National Insurance number");

        if (model.IsNassSelected == null && model.IsNinoSelected == false) return ValidationResult.Success;

        if (model.IsNassSelected == null && model.NASSRedirect)
            return new ValidationResult("Select yes if you have an Asylum support reference number");
        return ValidationResult.Success;
    }
}