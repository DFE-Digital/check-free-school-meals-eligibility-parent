using System.Text;
using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface IPerformEligibilityCheckUseCase
{
    Task<(CheckEligibilityResponse Response, string ResponseCode)> Execute(
        Parent parentRequest,
        ISession session
    );
}

public class PerformEligibilityCheckUseCase : IPerformEligibilityCheckUseCase
{
    private readonly ICheckGateway _checkGateway;

    public PerformEligibilityCheckUseCase(ICheckGateway checkGateway)
    {
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public async Task<(CheckEligibilityResponse Response, string ResponseCode)> Execute(
        Parent parentRequest,
        ISession session)
    {
        //
        // If user says “I have NO NINO” but “haven’t finished NASS page yet” => redirect to "Nass".
        //
        if (parentRequest.IsNinoSelected == false && parentRequest.IsNassSelected == null) return (null, "Nass");

        if (parentRequest.IsNassSelected == false) return (null, "Could_Not_Check");

        //
        // Otherwise, do ECS check => store session => (true, response, "Loader").
        //   - This includes NINO flow (IsNinoSelected == true).
        //   - It also includes NASS flow (IsNinoSelected == false) but 
        //     user has finished NASS page => (IsNassSelected == true).
        //
        session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parentRequest.FirstName ?? string.Empty));
        session.Set("ParentLastName", Encoding.UTF8.GetBytes(parentRequest.LastName ?? string.Empty));

        // Build DOB string
        var dobString = new DateOnly(
            int.Parse(parentRequest.Year),
            int.Parse(parentRequest.Month),
            int.Parse(parentRequest.Day)
        ).ToString("yyyy-MM-dd");

        session.Set("ParentDOB", Encoding.UTF8.GetBytes(dobString));

        // If we're finishing a NASS flow, store "ParentNASS"; 
        // otherwise store "ParentNINO".
        if (parentRequest.NationalAsylumSeekerServiceNumber?.Length > 0)
        {
            session.Set("ParentNASS", Encoding.UTF8.GetBytes(parentRequest.NationalAsylumSeekerServiceNumber ?? ""));
            session.Remove("ParentNINO");
        }
        else
        {
            session.Set("ParentNINO", Encoding.UTF8.GetBytes(parentRequest.NationalInsuranceNumber ?? ""));
            session.Remove("ParentNASS");
        }

        // Build ECS request
        var checkEligibilityRequest = new CheckEligibilityRequest_Fsm
        {
            Data = new CheckEligibilityRequestData_Fsm
            {
                LastName = parentRequest.LastName,
                NationalInsuranceNumber = parentRequest.NationalInsuranceNumber?.ToUpper(),
                NationalAsylumSeekerServiceNumber = parentRequest.NationalAsylumSeekerServiceNumber?.ToUpper(),
                DateOfBirth = dobString
            }
        };

        // Call ECS check


        var response = await _checkGateway.PostCheck(checkEligibilityRequest);

        return (response, "Success");
    }
}