using CsvHelper.Configuration.Attributes;

namespace CheckYourEligibility.Admin.Models;

public class EligibilityCheckReportCsvExport
{
    [Name("Parent Surname")]
    public string ParentSurname { get; set; }

    [Name("National Insurance Number")]
    public string NationalInsuranceNumber { get; set; }

    [Name("Date of Birth")]
    public string DateOfBirth { get; set; }

    [Name("Date check submitted")]
    public string DateCheckSubmitted { get; set; }

    [Name("Check Type")]
    public string CheckType { get; set; }

    [Name("Checked By")]
    public string CheckedBy { get; set; }

    [Name("Outcome")]
    public string Outcome { get; set; }
}