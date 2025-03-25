// Ignore Spelling: Fsm

namespace CheckYourEligibility.Admin.Domain.Enums;

public enum CheckEligibilityStatus
{
    queuedForProcessing,
    parentNotFound,
    eligible,
    notEligible,
    error
}