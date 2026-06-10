// Ignore Spelling: Fsm

using System.ComponentModel;

namespace CheckYourEligibility.Admin.Domain.Enums;

public enum ApplicationStatus
{
    Entitled,
    Receiving,
    EvidenceNeeded,
    SentForReview,
    ReviewedEntitled,
    ReviewedNotEntitled,
    Archived
}