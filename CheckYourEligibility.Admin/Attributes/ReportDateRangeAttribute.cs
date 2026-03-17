using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ReportDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = (EligibilityCheckReportViewModel)validationContext.ObjectInstance;

        var errorFields = new List<string>();

        var sd = model.StartDay;
        var sm = model.StartMonth;
        var sy = model.StartYear;

        var ed = model.EndDay;
        var em = model.EndMonth;
        var ey = model.EndYear;

        bool startMissing = sd == 0 || sm == 0 || sy == 0;
        bool endMissing = ed == 0 || em == 0 || ey == 0;

        if (startMissing)
        {
            errorFields.Add("StartDate");
            errorFields.Add("StartDay");
            errorFields.Add("StartMonth");
            errorFields.Add("StartYear");
            return new ValidationResult("Enter a complete start date", errorFields);
        }

        if (endMissing)
        {
            errorFields.Add("EndDate");
            errorFields.Add("EndDay");
            errorFields.Add("EndMonth");
            errorFields.Add("EndYear");
            return new ValidationResult("Enter a complete end date", errorFields);
        }

        if (sm < 1 || sm > 12) errorFields.Add("StartMonth");
        if (sd < 1 || sd > 31) errorFields.Add("StartDay");
        if (sy < 1900) errorFields.Add("StartYear");

        if (em < 1 || em > 12) errorFields.Add("EndMonth");
        if (ed < 1 || ed > 31) errorFields.Add("EndDay");
        if (ey < 1900) errorFields.Add("EndYear");

        if (errorFields.Count > 0)
        {
            errorFields.Insert(0, "StartDate");
            errorFields.Insert(0, "EndDate");
            return new ValidationResult("Enter valid dates", errorFields);
        }
        if (model.StartDateValue == null)
        {
            return new ValidationResult(
                "Start date must be a real date",
                new[] { "StartDate", "StartDay", "StartMonth", "StartYear" }
            );
        }
        if (model.EndDateValue == null)
        {
            return new ValidationResult(
                "End date must be a real date",
                new[] { "EndDate", "EndDay", "EndMonth", "EndYear" }
            );
        }
        var start = model.StartDateValue.Value;
        var end = model.EndDateValue.Value;
        if (end > DateTime.Today)
        {
            return new ValidationResult(
                "End date cannot be in the future",
                new[] { "EndDate", "EndDay", "EndMonth", "EndYear" }
            );
        }
        if (start > end)
        {
            return new ValidationResult(
                "Start date must be before or the same as the end date",
                new[] { "StartDate", "StartDay", "StartMonth", "StartYear",
                        "EndDate", "EndDay", "EndMonth", "EndYear" }
            );
        }
        if ((end - start).TotalDays > 365)
        {
            return new ValidationResult(
                "The date range cannot be longer than one year",
                new[] { "StartDate", "StartDay", "StartMonth", "StartYear",
                        "EndDate", "EndDay", "EndMonth", "EndYear" }
            );
        }
        return ValidationResult.Success;
    }
}
