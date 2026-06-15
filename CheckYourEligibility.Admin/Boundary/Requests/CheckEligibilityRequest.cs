using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    protected CheckEligibilityType baseType = CheckEligibilityType.FreeSchoolMeals;
    public string? NationalInsuranceNumber { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public int? Sequence { get; set; }
}

public interface IEligibilityServiceType
{
}

#region FreeSchoolMeals Enhanced Type

public class CheckEligibilityRequestData_Enhanced : CheckEligibilityRequestDataBase
{
    public string ChildFirstName { get; set; }
    public string ChildLastName { get; set; }
    public string ChildDateOfBirth   { get; set; }
    public string ChildSchoolUrn { get; set; }

}
public class CheckEligibilityRequest_Enhanced
{
    public CheckEligibilityRequestData_Enhanced? Data { get; set; }
}

public class CheckEligibilityRequestBulk_Fsm
{
    public IEnumerable<CheckEligibilityRequestData_Enhanced> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestData_Enhanced>();
    public CheckEligibilityRequestBulkMeta Meta { get; set; } = new();
}

#endregion

#region FSM Type

public class CheckEligibilityRequest
{
    public CheckEligibilityRequestDataBase? Data { get; set; }
}

public class CheckEligibilityRequestBulk
{
    public IEnumerable<CheckEligibilityRequestDataBase> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestDataBase>();
    public CheckEligibilityRequestBulkMeta Meta { get; set; } = new();
}

public class CheckEligibilityRequestBulkMeta
{
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string? LocalAuthorityId { get; set; }
}

#endregion