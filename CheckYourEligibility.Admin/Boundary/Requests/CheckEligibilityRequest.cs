using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    protected CheckEligibilityType baseType = CheckEligibilityType.FreeSchoolMeals;
    public string? NationalInsuranceNumber { get; set; }
    public string ParentLastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public int? Sequence { get; set; }
}
public class CheckEligibilityRequestBulkBase {

    public CheckEligibilityRequestBulkMeta Meta { get; set; } = new();
}

public interface IEligibilityServiceType
{
}

#region FreeSchoolMeals Enhanced Type

public class CheckEligibilityRequestData_Enhanced : CheckEligibilityRequestDataBase
{
    public string ParentFirstName { get;set; }
    public string ChildFirstName { get; set; }
    public string ChildLastName { get; set; }
    public string ChildDateOfBirth   { get; set; }
    public string ChildSchoolUrn { get; set; }

}
public class CheckEligibilityRequest_Enhanced
{
    public CheckEligibilityRequestData_Enhanced? Data { get; set; }
}

public class CheckEligibilityRequestBulk_Enhanced : CheckEligibilityRequestBulkBase
{
    public IEnumerable<CheckEligibilityRequestData_Enhanced> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestData_Enhanced>();
}

#endregion

#region FSM Type

public class CheckEligibilityRequest
{
    public CheckEligibilityRequestDataBase? Data { get; set; }
}

public class CheckEligibilityRequestBulk : CheckEligibilityRequestBulkBase
{
    public IEnumerable<CheckEligibilityRequestDataBase> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestDataBase>();
}

public class CheckEligibilityRequestBulkMeta
{
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string? LocalAuthorityId { get; set; }
}

#endregion