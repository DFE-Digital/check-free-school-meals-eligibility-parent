using System;

namespace CheckYourEligibility.Admin.Domain;

public class JwtAuthResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
}
