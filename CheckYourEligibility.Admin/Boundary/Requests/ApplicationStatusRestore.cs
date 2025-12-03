// Ignore Spelling: Fsm

using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Requests;

public class ApplicationStatusRestoreRequest
{
    public ApplicationStatusData? Data { get; set; }
}

public class ApplicationStatusRestoreData
{
    public ApplicationStatus Status { get; set; }
}