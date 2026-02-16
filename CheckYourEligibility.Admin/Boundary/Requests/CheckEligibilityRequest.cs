using CheckYourEligibility.Admin.Domain.Enums;

namespace CheckYourEligibility.Admin.Boundary.Requests;

public class CheckEligibilityRequestDataBase : IEligibilityServiceType
{
    protected CheckEligibilityType baseType;
    public int? Sequence { get; set; }
}

public interface IEligibilityServiceType
{
}

#region FreeSchoolMeals Type

public class CheckEligibilityRequestData_Fsm : CheckEligibilityRequestDataBase
{
    public CheckEligibilityRequestData_Fsm()
    {
        baseType = CheckEligibilityType.FreeSchoolMeals;
    }

    public string? NationalInsuranceNumber { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public string? NationalAsylumSeekerServiceNumber { get; set; }
}

public class CheckEligibilityRequest_Fsm
{
    public CheckEligibilityRequestData_Fsm? Data { get; set; }
}

public class CheckEligibilityRequestBulk_Fsm
{
    public IEnumerable<CheckEligibilityRequestData_Fsm> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestData_Fsm>();
    public CheckEligibilityRequestBulkMeta Meta { get; set; } = new();
}

#endregion

#region FSM Basic Type

public class CheckEligibilityRequestData_FsmBasic : CheckEligibilityRequestDataBase
{
    public CheckEligibilityRequestData_FsmBasic()
    {
        baseType = CheckEligibilityType.FreeSchoolMeals;
    }

    public string? NationalInsuranceNumber { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public string? NationalAsylumSeekerServiceNumber { get; set; }
}

public class CheckEligibilityRequest_FsmBasic
{
    public CheckEligibilityRequestData_FsmBasic? Data { get; set; }
}

public class CheckEligibilityRequestBulk_FsmBasic
{
    public IEnumerable<CheckEligibilityRequestData_FsmBasic> Data { get; set; } = Enumerable.Empty<CheckEligibilityRequestData_FsmBasic>();
    public CheckEligibilityRequestBulkMeta Meta { get; set; } = new();
}

public class CheckEligibilityRequestBulkMeta
{
    public string Filename { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string? LocalAuthorityId { get; set; }
}

#endregion