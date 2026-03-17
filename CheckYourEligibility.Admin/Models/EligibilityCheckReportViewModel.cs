using CheckYourEligibility.Admin.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

[ReportDate]
public class EligibilityCheckReportViewModel
{
    [Required]
    public int StartDay { get; set; }

    [Required]
    public int StartMonth { get; set; }

    [Required]
    public int StartYear { get; set; }

    [Required]
    public int EndDay { get; set; }

    [Required]
    public int EndMonth { get; set; }

    [Required]
    public int EndYear { get; set; }

    [Required]
    public CheckType CheckType { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }

    public DateTime? StartDateValue =>
        DateTime.TryParse($"{StartYear}-{StartMonth}-{StartDay}", out var d) ? d : null;

    public DateTime? EndDateValue =>
        DateTime.TryParse($"{EndYear}-{EndMonth}-{EndDay}", out var d) ? d : null;
}
