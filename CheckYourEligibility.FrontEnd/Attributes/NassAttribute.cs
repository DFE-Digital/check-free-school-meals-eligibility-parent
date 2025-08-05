using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class NassAttribute : ValidationAttribute
{
    //111911111
    private static readonly string NassPattern = @"^[0-9]{2}(0[1-9]|1[0-2])[0-9]{5,6}$";
    private static readonly Regex regex = new(NassPattern);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;

        if (model.IsNassSelected == true)
        {
            if (value == null || value == "") return new ValidationResult("Nass is required");

            if (!regex.IsMatch(value.ToString()))
                return new ValidationResult("Nass field contains an invalid character");
        }

        return ValidationResult.Success;
    }
}