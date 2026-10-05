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
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return DateTime.TryParseExact(
            value,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out _);
    }

    internal static bool BeAValidNass(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        const string regexString =
            @"^[0-9]{2}(0[1-9]|1[0-2])[0-9]{5,6}$";

        return Regex.IsMatch(value, regexString);
    }
}
