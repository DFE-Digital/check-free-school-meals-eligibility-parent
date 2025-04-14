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
    private readonly IBlobStorageGateway _blobStorageGateway;
    private const string EvidenceContainerName = "fsm-evidence";

    public SubmitApplicationUseCase(
        ILogger<SubmitApplicationUseCase> logger,
        IParentGateway parentGateway,
        IBlobStorageGateway blobStorageGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parentGateway = parentGateway ?? throw new ArgumentNullException(nameof(parentGateway));
        _blobStorageGateway = blobStorageGateway ?? throw new ArgumentNullException(nameof(blobStorageGateway));
    }

    public async Task<List<ApplicationSaveItemResponse>> Execute(
        FsmApplication request,
        string userId,
        string establishment)
    {
        var responses = new List<ApplicationSaveItemResponse>();
        var evidenceList = await ProcessEvidenceFilesAsync(request);

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
                    Evidence = evidenceList
                }
            };
            var response = await _parentGateway.PostApplication_Fsm(application);
            responses.Add(response);
        }

        return responses;
    }

    private async Task<List<ApplicationEvidence>> ProcessEvidenceFilesAsync(FsmApplication request)
    {
        var evidenceList = new List<ApplicationEvidence>();
        if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
        {
            foreach (var file in request.EvidenceFiles)
            {
                try
                {
                    
                    string blobUrl = await _blobStorageGateway.UploadFileAsync(file, EvidenceContainerName);
                    
                    evidenceList.Add(new ApplicationEvidence
                    {
                        FileName = file.FileName,
                        FileType = file.ContentType,
                        StorageAccountReference = blobUrl
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload evidence file {FileName}", file.FileName);
                    // TODO: Handle the error as needed
                }
            }
        }

        return evidenceList;
    }
}