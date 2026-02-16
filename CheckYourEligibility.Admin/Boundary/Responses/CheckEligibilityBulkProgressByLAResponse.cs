namespace CheckYourEligibility.Admin.Boundary.Responses;

public class CheckEligibilityBulkProgressByLAResponse
{
    public IEnumerable<CheckEligibilityBulkProgressResponse> Checks { get; set; }
}

public class CheckEligibilityBulkProgressResponse
{
    public string Id { get; set; }
    public string Filename { get; set; }
    public int? NumberOfRecords { get; set; }
    public string Status { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string SubmittedBy { get; set; }
    public string EligibilityType { get; set; }
    public string? FinalNameInCheck { get; set; }
}
