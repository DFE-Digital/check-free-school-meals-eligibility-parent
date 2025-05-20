using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class NameAttribute : ValidationAttribute
{
    public static readonly string UnicodeOnlyPattern = @"^[a-zA-Z ,.'-]+$";

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
                return new ValidationResult("Enter a first name with valid characters");
        }

        if (lastName == value)
        {
            if (value == null || value == "")
                return ValidationResult.Success;

            if (!regex.IsMatch(value.ToString()))
                return new ValidationResult("Enter a last name with valid characters");
        }

        return ValidationResult.Success;
    }
}