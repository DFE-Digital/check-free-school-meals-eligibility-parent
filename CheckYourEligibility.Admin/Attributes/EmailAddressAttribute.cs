using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckYourEligibility.Admin.Models;

public class EmailAddressAttribute : ValidationAttribute
{
    private const string LocalPartPattern = @"^[a-zA-Z0-9._'+-]+$";

    private const string DomainPartPattern =
        @"^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // skip this validation if this is for searching application records
        var parentObject = validationContext.ObjectInstance.GetType().GetProperty(validationContext.MemberName)
            ?.DeclaringType;

        if (parentObject == typeof(ApplicationSearch)) return ValidationResult.Success;


        if (value == null) return ValidationResult.Success;

        if (!(value is string valueAsString))
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");

        if (valueAsString.Contains(' ') || valueAsString.Length > 320)
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");

        var index = valueAsString.IndexOf('@');
        if (index <= 0 || index == valueAsString.Length - 1 || index != valueAsString.LastIndexOf('@'))
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");

        var localPart = valueAsString.Substring(0, index);
        var domainPart = valueAsString.Substring(index + 1);

        if (localPart.Length > 64 || domainPart.Length > 255)
            return new ValidationResult("Enter an email address in the correct format, like name@example.com");

        if (!IsValidLocalPart(localPart) || !IsValidDomainPart(domainPart))
        {
            if (ContainsUnicodeCharacters(domainPart))
            {
                if (!IsValidInternationalizedDomainPart(domainPart))
                    return new ValidationResult("Enter an email address in the correct format, like name@example.com");
            }
            else
            {
                return new ValidationResult("Enter an email address in the correct format, like name@example.com");
            }
        }

        return ValidationResult.Success;
    }

    private bool IsValidLocalPart(string localPart)
    {
        if (!Regex.IsMatch(localPart, LocalPartPattern))
            return false;

        if (localPart.StartsWith(".") || localPart.EndsWith("."))
            return false;

        return true;
    }

    private bool IsValidDomainPart(string domainPart)
    {
        if (!Regex.IsMatch(domainPart, DomainPartPattern))
            return false;

        if (domainPart.StartsWith(".") || domainPart.StartsWith("-") ||
            domainPart.EndsWith(".") || domainPart.EndsWith("-"))
            return false;

        return true;
    }

    private bool ContainsUnicodeCharacters(string text)
    {
        return text.Any(c => c > 127);
    }

    private bool IsValidInternationalizedDomainPart(string domainPart)
    {
        if (!domainPart.Contains("."))
            return false;

        if (domainPart.StartsWith(".") || domainPart.StartsWith("-") ||
            domainPart.EndsWith(".") || domainPart.EndsWith("-"))
            return false;

        // Check for consecutive dots
        if (domainPart.Contains(".."))
            return false;

        return true;
    }
}