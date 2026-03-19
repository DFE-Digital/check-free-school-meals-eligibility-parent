using System.ComponentModel.DataAnnotations;

public class ReportDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var model = (EligibilityCheckReportViewModel)context.ObjectInstance;

        bool startMissing = !model.StartDay.HasValue ||
                            !model.StartMonth.HasValue ||
                            !model.StartYear.HasValue;

        if (startMissing)
        {
            return new ValidationResult(
                "Enter a complete start date",
                new[] { "StartDate", "StartDate.Day", "StartDate.Month", "StartDate.Year" }
            );
        }

        if (model.StartDay < 1 || model.StartDay > 31)
        {
            return new ValidationResult(
                "Enter a valid day",
                new[] { "StartDate.Day" }
            );
        }

        if (model.StartMonth < 1 || model.StartMonth > 12)
        {
            return new ValidationResult(
                "Enter a valid month",
                new[] { "StartDate.Month" }
            );
        }

        if (model.StartYear < 1900 || model.StartYear > DateTime.Now.Year)
        {
            return new ValidationResult(
                "Enter a valid year",
                new[] { "StartDate.Year" }
            );
        }

        bool endMissing = !model.EndDay.HasValue ||
                          !model.EndMonth.HasValue ||
                          !model.EndYear.HasValue;

        if (endMissing)
        {
            return new ValidationResult(
                "Enter a complete end date",
                new[] { "EndDate", "EndDate.Day", "EndDate.Month", "EndDate.Year" }
            );
        }

        if (model.EndDay < 1 || model.EndDay > 31)
        {
            return new ValidationResult(
                "Enter a valid day",
                new[] { "EndDate.Day" }
            );
        }

        if (model.EndMonth < 1 || model.EndMonth > 12)
        {
            return new ValidationResult(
                "Enter a valid month",
                new[] { "EndDate.Month" }
            );
        }

        if (model.EndYear < 1900 || model.EndYear > DateTime.Now.Year)
        {
            return new ValidationResult(
                "Enter a valid year",
                new[] { "EndDate.Year" }
            );
        }

        return ValidationResult.Success;
    }
}
