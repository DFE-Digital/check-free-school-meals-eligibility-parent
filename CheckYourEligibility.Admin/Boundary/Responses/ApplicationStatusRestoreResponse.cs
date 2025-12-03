// Ignore Spelling: Fsm

namespace CheckYourEligibility.Admin.Boundary.Responses;

public class ApplicationStatusRestoreResponse
{
    public ApplicationStatusRestoreDataResponse Data { get; set; }
}

public class ApplicationStatusRestoreDataResponse
{
    public string Status { get; set; }
    public string Updated { get; set; }
}