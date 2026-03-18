using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

public class EligibilityCheckReportViewModel : IValidatableObject
{
    public int? StartDay { get; set; }
    public int? StartMonth { get; set; }
    public int? StartYear { get; set; }

    public int? EndDay { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }

    [BindNever]
    [ValidateNever]
    public string StartDate { get; set; }

    [BindNever]
    [ValidateNever]
    public string EndDate { get; set; }

    public DateTime? StartDateValue =>
        StartDay.HasValue && StartMonth.HasValue && StartYear.HasValue
            ? TryCreateDate(StartYear.Value, StartMonth.Value, StartDay.Value)
            : null;

    public DateTime? EndDateValue =>
        EndDay.HasValue && EndMonth.HasValue && EndYear.HasValue
            ? TryCreateDate(EndYear.Value, EndMonth.Value, EndDay.Value)
            : null;

    private DateTime? TryCreateDate(int year, int month, int day)
    {
        try { return new DateTime(year, month, day); }
        catch { return null; }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // -------------------------
        // START DATE
        // -------------------------
        bool startMissing = !StartDay.HasValue || !StartMonth.HasValue || !StartYear.HasValue;

        if (startMissing)
        {
            yield return new ValidationResult(
                "Enter a complete start date",
                new[] { "StartDate", "StartDate.Day", "StartDate.Month", "StartDate.Year" }
            );
        }
        else
        {
            if (StartDay < 1 || StartDay > 31)
                yield return new ValidationResult("Enter a valid day", new[] { "StartDate", "StartDate.Day" });

            if (StartMonth < 1 || StartMonth > 12)
                yield return new ValidationResult("Enter a valid month", new[] { "StartDate", "StartDate.Month" });

            if (StartYear < 1900 || StartYear > DateTime.Now.Year)
                yield return new ValidationResult("Enter a valid year", new[] { "StartDate", "StartDate.Year" });

            if (StartDateValue == null)
            {
                yield return new ValidationResult(
                    "Enter a real date",
                    new[] { "StartDate", "StartDate.Day", "StartDate.Month", "StartDate.Year" }
                );
            }

            if (StartDateValue > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date must be today or in the past",
                    new[] { "StartDate" }
                );
            }
        }
        // -------------------------
        // END DATE
        // -------------------------
        bool endMissing = !EndDay.HasValue || !EndMonth.HasValue || !EndYear.HasValue;

        if (endMissing)
        {
            yield return new ValidationResult(
                "Enter a complete end date",
                new[] { "EndDate", "EndDate.Day", "EndDate.Month", "EndDate.Year" }
            );
        }
        else
        {
            if (EndDay < 1 || EndDay > 31)
                yield return new ValidationResult("Enter a valid day", new[] { "EndDate", "EndDate.Day" });

            if (EndMonth < 1 || EndMonth > 12)
                yield return new ValidationResult("Enter a valid month", new[] { "EndDate", "EndDate.Month" });

            if (EndYear < 1900 || EndYear > DateTime.Now.Year)
                yield return new ValidationResult("Enter a valid year", new[] { "EndDate", "EndDate.Year" });

            if (EndDateValue == null)
            {
                yield return new ValidationResult(
                    "Enter a real date",
                    new[] { "EndDate", "EndDate.Day", "EndDate.Month", "EndDate.Year" }
                );
            }

            if (EndDateValue > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date must be today or in the past",
                    new[] { "EndDate" }
                );
            }
        }
        // -------------------------
        // RANGE CHECKS (only if both dates valid)
        // -------------------------
        if (StartDateValue.HasValue && EndDateValue.HasValue)
        {
            if (EndDateValue < StartDateValue)
            {
                yield return new ValidationResult(
                    "The end date must be after the start date",
                    new[] { "EndDate", "EndDate.Day", "EndDate.Month", "EndDate.Year" }
                );
            }

            if ((EndDateValue.Value - StartDateValue.Value).TotalDays > 365)
            {
                yield return new ValidationResult(
                    "The start date and end date must not be more than 12 months apart",
                    new[] { "EndDate", "StartDate" }
                );
            }
        }
    }
}
