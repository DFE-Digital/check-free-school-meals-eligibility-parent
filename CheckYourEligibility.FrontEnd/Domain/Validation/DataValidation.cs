using System.Text.RegularExpressions;

namespace CheckYourEligibility.API.Domain.Validation;

internal static class DataValidation
{
    internal static bool BeAValidNi(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string regexString =
            @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z][0-9]{6}[A-D]$";

        return Regex.IsMatch(
            value,
            regexString,
            RegexOptions.IgnoreCase);
    }

    internal static bool BeAValidDate(string value)
    {
        var regexString =
            @"^\d{4}-\d{2}-\d{2}$";
        var rg = new Regex(regexString);
        var res = rg.Match(value);
        return res.Success;
    }
}