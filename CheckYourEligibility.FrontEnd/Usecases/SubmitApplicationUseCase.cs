using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Boundary.Shared;
using CheckYourEligibility.FrontEnd.Domain.Enums;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface ISubmitApplicationUseCase
{
    Task<List<ApplicationSaveItemResponse>> Execute(
        FsmApplication request,
        string currentStatus,
        string userId,
        string email);
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
        string currentStatus,
        string userId,
        string email)
    {
        //_logger.LogInformation("Processing application with current eligibility status: {Status}", currentStatus);

        //if (currentStatus != CheckEligibilityStatus.eligible.ToString())
        //{
        //    _logger.LogError("Invalid status when trying to create an application: {Status}", currentStatus);
        //    throw new Exception($"Invalid status when trying to create an application: {currentStatus}");
        //}

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
                    ParentDateOfBirth = request.ParentDateOfBirth,
                    ParentNationalInsuranceNumber = request.ParentNino,
                    ParentNationalAsylumSeekerServiceNumber = request.ParentNass,
                    ChildFirstName = child.FirstName,
                    ChildLastName = child.LastName,
                    ChildDateOfBirth =
                        new DateOnly(int.Parse(child.Year), int.Parse(child.Month), int.Parse(child.Day)).ToString(
                            "yyyy-MM-dd"),
                    Establishment = int.Parse(child.School.URN),
                    UserId = userId,
                    Evidence = evidenceList.Count > 0 ? evidenceList : null,
                    ParentEmail = email
                }
            };
            var response = await _parentGateway.PostApplication_Fsm(application);
            responses.Add(response);
        }

        _logger.LogInformation("Successfully processed {Count} applications", responses.Count);
        return responses;
    }
}