// Ignore Spelling: Fsm

using System.ComponentModel;

namespace CheckYourEligibility.Admin.Domain.Enums;

public enum ApplicationStatus
{
    [Description("Entitled")] Entitled,
    [Description("Receiving entitlement")] Receiving,
    [Description("Evidence needed")] EvidenceNeeded,
    [Description("Sent for review")] SentForReview,
    [Description("Reviewed entitled")] ReviewedEntitled,
    [Description("Reviewed not entitled")] ReviewedNotEntitled,
    [Description("Archived")] Archived
}