using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Boundary.Shared;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.UseCases;

public interface ISubmitApplicationUseCase
{
    Task<List<ApplicationSaveItemResponse>> Execute(
        FsmApplication request,
        string userId,
        string establishment);
}

public class SubmitApplicationUseCase : ISubmitApplicationUseCase
{
    private readonly ILogger<SubmitApplicationUseCase> _logger;
    private readonly IParentGateway _parentGateway;

    public SubmitApplicationUseCase(
        ILogger<SubmitApplicationUseCase> logger,
        IParentGateway parentGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parentGateway = parentGateway ?? throw new ArgumentNullException(nameof(parentGateway));
    }

    public async Task<List<ApplicationSaveItemResponse>> Execute(
        FsmApplication request,
        string userId,
        string establishment)
    {
        var responses = new List<ApplicationSaveItemResponse>();
        
        List<ApplicationEvidence> evidenceList = new List<ApplicationEvidence>();
        if (request.Evidence?.EvidenceList != null && request.Evidence.EvidenceList.Any())
        {
            foreach (var evidenceFile in request.Evidence.EvidenceList)
            {
                evidenceList.Add(new ApplicationEvidence
                {
                    FileName = evidenceFile.FileName,
                    FileType = evidenceFile.FileType,
                    StorageAccountReference = evidenceFile.StorageAccountReference
                });
            }
        }


        foreach (var child in request.Children.ChildList)
        {
            var application = new ApplicationRequest
            {
                Data = new ApplicationRequestData
                {
                    Type = CheckEligibilityType.FreeSchoolMeals,
                    ParentFirstName = request.ParentFirstName,
                    ParentLastName = request.ParentLastName,
                    ParentEmail = request.ParentEmail,
                    ParentDateOfBirth = request.ParentDateOfBirth,
                    ParentNationalInsuranceNumber = request.ParentNino,
                    ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                    ChildFirstName = child.FirstName,
                    ChildLastName = child.LastName,
                    ChildDateOfBirth = new DateOnly(
                        int.Parse(child.Year),
                        int.Parse(child.Month),
                        int.Parse(child.Day)).ToString("yyyy-MM-dd"),
                    Establishment = int.Parse(establishment),
                    UserId = userId,
                    Evidence = evidenceList.Count > 0 ? evidenceList : null
                }
            };
            var response = await _parentGateway.PostApplication_Fsm(application);
            responses.Add(response);
        }

        return responses;
    }
}