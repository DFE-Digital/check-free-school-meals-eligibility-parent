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
        if (value == null || value == "")
            return ValidationResult.Success;

        if (regex.IsMatch(value.ToString()))
            return ValidationResult.Success;

        return validationContext.MemberName switch
        {
            "FirstName" => new ValidationResult("Enter a first name with valid characters"),
            "LastName" => new ValidationResult("Enter a last name with valid characters"),
            _ => new ValidationResult("Enter a name with valid characters")
        };
    }
}
