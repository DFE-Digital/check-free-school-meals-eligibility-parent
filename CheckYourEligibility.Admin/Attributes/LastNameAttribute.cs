using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckYourEligibility.Admin.Attributes
{
    public class LastNameAttribute : ValidationAttribute
    {
        private readonly string _fieldName;
        private readonly string _objectName;
        private readonly string _childIndexPropertyName;

        private static readonly string UnicodeOnlyPattern = @"^[\p{L}\-']+$";

        private static readonly Regex regex = new Regex(UnicodeOnlyPattern);

        public LastNameAttribute(string fieldName, string objectName, string? childIndexPropertyName, string? errorMessage = null) : base(errorMessage)
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

            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return ValidationResult.Success;
            }

            if (!regex.IsMatch(value.ToString()))
            {
                if (childIndex != null)
                {
                    return new ValidationResult($"{_fieldName} contains an invalid character for {_objectName} {childIndex}");
                }
                else if (model.GetType() == typeof(ApplicationSearch))
                {
                    return new ValidationResult($"{_objectName} {_fieldName} field contains an invalid character");
                }
                else
                {
                    return new ValidationResult($"{_fieldName} contains an invalid character");
                }
            }

            return ValidationResult.Success;
        }

        private int? GetPropertyIntValue(object model, string propertyName)
        {
            return model.GetType().GetProperty(propertyName)?.GetValue(model) as int?;
        }
    }
}
