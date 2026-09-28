using CheckYourEligibility.API.Domain.Validation;
using CheckYourEligibility.FrontEnd.Models;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class NinoAttribute : ValidationAttribute
{  
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;

        if (model.IsNinoSelected == null && value == null)
            return ValidationResult.Success;

        if (model.IsNinoSelected == false)
            return ValidationResult.Success;

        if (model.IsNinoSelected == true && value == null)
            return new ValidationResult("National Insurance number is required");

        var nino = value.ToString();

        if (!DataValidation.BeAValidNi(nino))
            return new ValidationResult("Enter a National Insurance number in the correct format");

        model.NationalInsuranceNumber = nino.ToUpperInvariant();

        return ValidationResult.Success;
    }
}
