using System;

namespace CheckYourEligibility.Admin.Domain;

public class SystemUser
{
    public string? Identifier { get; set; }  // Can store either client_id or username
    public string? Secret { get; set; }      // Can store either client_secret or password
    public string? scope { get; set; }
    public string? grant_type { get; set; }

    public string? client_id { get; set; }
    public string? client_secret { get; set; }

    // Legacy properties for backward compatibility
    public string? Username { get; set; }
    public string? Password { get; set; }
}
