using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CheckYourEligibility.FrontEnd.Attributes;

public class NameAttribute : ValidationAttribute
{
    public static readonly string NameValidationRegex = @"^[a-zA-Z" +
            @"ÁáÉéÍíÓóÚúÝýĆćĹĺŃńŔŕŚśŹź" +
            @"ÀàÈèÌìÒòÙùẀẁỲỳ" +
            @"ÂâÊêÎîÔôÛûĈĉĜĝĤĥĴĵŜŝŴŵŶŷ" +
            @"ÃãÑñÕõĨĩŨũẼẽỸỹ" +
            @"ÄäËëÏïÖöÜüŸÿ" +
            @"ÇçĢģĶķĻļŅņŖŗŞşŢţ" +
            @"ÅåŮů" +
            @"ĀāĒēĪīŌōŪūȲȳ" +
            @"ĂăĔĕĞğĬĭŎŏŬŭ" +
            @"ĊċĖėĠġİẊẋŻż" +
            @"ĄąĘęĮįŲų" +
            @"ŐőŰű" +
            @" ,.''\u2018\u2019-]+$";

    private static readonly Regex regex = new(NameValidationRegex);

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