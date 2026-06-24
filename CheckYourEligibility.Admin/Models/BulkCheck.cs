namespace CheckYourEligibility.Admin.Models;

public class BulkCheck
{
    public string BulkCheckId { get; set; }
    public string Filename { get; set; }
    public string EligibilityType { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string SubmittedBy { get; set; }
    public string Status { get; set; }
    public int? NumberOfRecords { get; set; }
    public string? FinalNameInCheck { get; set; }
}

public interface IBulkExport
{
}

public class BulkExportBase : IBulkExport
{

    public string LastName { get; set; }
    public string DateOfBirth { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public string Outcome { get; set; }
    public string? EligibilityEndDate { get; set; }
    public string? Tier { get; set; }
}

public class BulkExport: BulkExportBase
{
    public string FirstName { get; set; }

    public string ChildFirstName { get; set; }

    public string ChildLastName { get; set; }
    public string ChildDateOfBirth  { get; set; }

    public string ChildSchoolUrn { get; set; }

}
