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

public class BulkExport : IBulkExport
{
    public string LastName { get; set; }
    public string DOB { get; set; }
    public string NI { get; set; }
    public string Outcome { get; set; }
}

public class BulkExportWorkingFamilies : IBulkExport
{
    public string EligibilityCode { get; set; }
    public string ChildDOB { get; set; }
    public string NI { get; set; }
    public string ValidityStartDate { get; set; }
    public string ValidityEndDate { get; set; }
    public string GracePeriodEnds { get; set; }
    public string Outcome { get; set; }
}
