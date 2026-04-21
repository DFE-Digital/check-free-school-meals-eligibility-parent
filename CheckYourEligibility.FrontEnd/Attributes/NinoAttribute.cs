using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class NinoAttribute : ValidationAttribute
{
    private static readonly string FirstLetterPattern = "[ABCEGHJKLMNOPRSTWXYZ]";
    private static readonly string SecondLetterPattern = "[ABCEGHJKLMNPRSTWXYZ]";
    private static readonly string DisallowedPrefixesPattern = "^(?!BG|GB|KN|NK|NT|TN|ZZ)";
    private static readonly string NumericPattern = "[0-9]{6}";
    private static readonly string LastLetterPattern = "[ABCD]";

    private static readonly string Pattern = DisallowedPrefixesPattern + FirstLetterPattern + SecondLetterPattern +
                                             NumericPattern + LastLetterPattern;

    private static readonly Regex regex = new(Pattern);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (Parent)validationContext.ObjectInstance;

        if (model.IsNinoSelected == null && value == null) return ValidationResult.Success;

        if (model.IsNinoSelected == false) return ValidationResult.Success;

        if (model.IsNinoSelected == true && value == null)
            return new ValidationResult("National Insurance number is required");

        var nino = value.ToString().ToUpper();
        nino = string.Concat(nino
            .Where(ch => char.IsLetterOrDigit(ch)));

        if (nino.Length > 9)
            return new ValidationResult(
                "National Insurance number should contain no more than 9 alphanumeric characters");

        if (!regex.IsMatch(nino)) return new ValidationResult("Enter a National Insurance number in the correct format");

        model.NationalInsuranceNumber = nino;

        return ValidationResult.Success;
    }
}