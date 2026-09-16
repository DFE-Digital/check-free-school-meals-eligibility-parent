// Ignore Spelling: Fsm

namespace CheckYourEligibility.FrontEnd.Boundary.Requests;

public class UserCreateRequest
{
    public UserData? Data { get; set; }
    public CheckMetaData? MetaData { get; set; }
}

public class UserData
{
    public string Email { get; set; }
    public string Reference { get; set; }
}

public class CheckMetaData
{
    public string? Source { get; set; }
    public string? UserName { get; set; }
    public int? OrganisationID { get; set; }
    public string? OrganisationType { get; set; }
}