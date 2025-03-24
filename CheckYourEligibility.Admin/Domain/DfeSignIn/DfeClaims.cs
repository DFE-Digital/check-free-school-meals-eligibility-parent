namespace CheckYourEligibility.Admin.Domain.DfeSignIn;

public sealed class DfeClaims
{
    public Organisation? Organisation { get; set; }
    public UserInformation User { get; set; }
}

public sealed class UserInformation
{
    public string Email { get; set; }
    public string Id { get; internal set; }
    public string FirstName { get; internal set; }
    public string Surname { get; internal set; }
}
