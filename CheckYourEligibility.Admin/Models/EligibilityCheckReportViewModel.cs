using CheckYourEligibility.Admin.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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
}
