using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility.Admin.Attributes;

public class NameAttribute : ValidationAttribute
{
    private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";

    private static readonly Regex regex = new(UnicodeOnlyPattern);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;

        var firstName = model.GetType().GetProperty("FirstName").GetValue(model);
        var lastName = model.GetType().GetProperty("LastName").GetValue(model);

        if (firstName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
                return new ValidationResult("First Name field contains an invalid character");
        }

        if (lastName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
                return new ValidationResult("Last Name field contains an invalid character");
        }

        return ValidationResult.Success;
    }
}