using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Attributes
{
    public class YearAttribute : ValidationAttribute
    {
        int allowableYearInPast = DateTime.Now.Year - 140;
        int currentYear = DateTime.Now.Year;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value == "")
            {
                return ValidationResult.Success;
            }

            if ((int)value > currentYear || (int)value < allowableYearInPast)
            {
                return new ValidationResult("Invalid Year");
            }

            return ValidationResult.Success;
        }
    }
}
