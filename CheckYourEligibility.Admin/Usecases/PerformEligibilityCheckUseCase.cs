using System.Text;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IPerformEligibilityCheckUseCase
    {
        Task<CheckEligibilityResponse> Execute(
            ParentGuardian parentRequest,
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

        public async Task<CheckEligibilityResponse> Execute(
            ParentGuardian parentRequest,
            ISession session)
        {
            session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parentRequest.FirstName ?? string.Empty));
            session.Set("ParentLastName", Encoding.UTF8.GetBytes(parentRequest.LastName ?? string.Empty));

            // Build DOB string
            var dobString = new DateOnly(
                int.Parse(parentRequest.Year),
                int.Parse(parentRequest.Month),
                int.Parse(parentRequest.Day)
            ).ToString("yyyy-MM-dd");

            session.Set("ParentDOB", Encoding.UTF8.GetBytes(dobString));
            session.SetString("ParentEmail", parentRequest.EmailAddress);

            // If we're finishing a NASS flow, store "ParentNASS"; 
            // otherwise store "ParentNINO".
            if (parentRequest.NinAsrSelection == ParentGuardian.NinAsrSelect.AsrnSelected)
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

            return response;
        }
    }
}
