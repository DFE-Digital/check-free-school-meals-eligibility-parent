// Ignore Spelling: Fsm

using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Requests;

public class ApplicationStatusUpdateRequest
{
    public ApplicationStatusData? Data { get; set; }
}

public class ApplicationStatusData
{
    public ApplicationStatus Status { get; set; }
}