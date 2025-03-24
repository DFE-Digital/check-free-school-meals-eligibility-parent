using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility.Admin.Attributes
{
    public class ReferenceNumberAttribute : ValidationAttribute
    {
        private static readonly string UnicodeOnlyPattern = @"^\d+$";

        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) 
            {
                return ValidationResult.Success;
            }
            if (!regex.IsMatch(value.ToString()))
            {
                return new ValidationResult("Reference Number field contains an invalid character");
            }

            return ValidationResult.Success;
                
        }
    }
}
