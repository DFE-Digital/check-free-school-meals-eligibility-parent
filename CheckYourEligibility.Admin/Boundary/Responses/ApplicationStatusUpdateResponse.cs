// Ignore Spelling: Fsm

namespace CheckYourEligibility.Admin.Boundary.Responses;

public class ApplicationStatusUpdateResponse
{
    public ApplicationStatusDataResponse Data { get; set; }
}

public class ApplicationStatusDataResponse
{
    public string Status { get; set; }
}