using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.Attributes;

public class LastNameAttribute : ValidationAttribute
{
    private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";

    private static readonly Regex regex = new(UnicodeOnlyPattern);
    private readonly string _childIndexPropertyName;
    private readonly string _fieldName;
    private readonly string _objectName;

    public LastNameAttribute(string fieldName, string objectName, string? childIndexPropertyName,
        string? errorMessage = null) : base(errorMessage)
    {
        _fieldName = fieldName;
        _objectName = objectName;
        _childIndexPropertyName = childIndexPropertyName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        int? childIndex = null;

        // get the child index if it exists, this should return null only if the model is ParentGuardian
        if (model.GetType() == typeof(Child))
        {
            model = validationContext.ObjectInstance as Child;
            childIndex = GetPropertyIntValue(model, _childIndexPropertyName);
        }

        if (string.IsNullOrEmpty(value?.ToString())) return ValidationResult.Success;

        if (!regex.IsMatch(value.ToString()))
        {
            if (childIndex != null)
                return new ValidationResult(
                    $"{_fieldName} contains an invalid character for {_objectName} {childIndex}");

            if (model.GetType() == typeof(ApplicationSearch))
                return new ValidationResult($"{_objectName} {_fieldName} field contains an invalid character");

            return new ValidationResult($"{_fieldName} contains an invalid character");
        }

        return ValidationResult.Success;
    }

    private int? GetPropertyIntValue(object model, string propertyName)
    {
        return model.GetType().GetProperty(propertyName)?.GetValue(model) as int?;
    }
}