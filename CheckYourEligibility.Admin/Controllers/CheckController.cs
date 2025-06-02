using Azure;
using Azure.Core;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.UseCases;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using Child = CheckYourEligibility.Admin.Models.Child;

namespace CheckYourEligibility.Admin.Controllers;

public class CheckController : BaseController
{
    private readonly IAddChildUseCase _addChildUseCase;
    private readonly IChangeChildDetailsUseCase _changeChildDetailsUseCase;
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;
    private readonly ICreateUserUseCase _createUserUseCase;
    private readonly IEnterChildDetailsUseCase _enterChildDetailsUseCase;
    private readonly IGetCheckStatusUseCase _getCheckStatusUseCase;
    private readonly ILoadParentDetailsUseCase _loadParentDetailsUseCase;
    private readonly ILogger<CheckController> _logger;
    private readonly IParentGateway _parentGateway;
    private readonly IPerformEligibilityCheckUseCase _performEligibilityCheckUseCase;
    private readonly IProcessChildDetailsUseCase _processChildDetailsUseCase;
    private readonly IRemoveChildUseCase _removeChildUseCase;
    private readonly ISubmitApplicationUseCase _submitApplicationUseCase;
    private readonly IValidateParentDetailsUseCase _validateParentDetailsUseCase;
    private readonly IUploadEvidenceFileUseCase _uploadEvidenceFileUseCase;
    private readonly IValidateEvidenceFileUseCase _validateEvidenceFileUse;
    private readonly ISendNotificationUseCase _sendNotificationUseCase;
    private readonly IDeleteEvidenceFileUseCase _deleteEvidenceFileUseCase;


    public CheckController(
        ILogger<CheckController> logger,
        IParentGateway parentGateway,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        IPerformEligibilityCheckUseCase performEligibilityCheckUseCase,
        IEnterChildDetailsUseCase enterChildDetailsUseCase,
        IProcessChildDetailsUseCase processChildDetailsUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        IAddChildUseCase addChildUseCase,
        IRemoveChildUseCase removeChildUseCase,
        IChangeChildDetailsUseCase changeChildDetailsUseCase,
        ICreateUserUseCase createUserUseCase,
        ISubmitApplicationUseCase submitApplicationUseCase,
        IValidateParentDetailsUseCase validateParentDetailsUseCase,
        IUploadEvidenceFileUseCase uploadEvidenceFileUseCase,
        IValidateEvidenceFileUseCase validateEvidenceFileUseCase,
        ISendNotificationUseCase sendNotificationUseCase,
        IDeleteEvidenceFileUseCase deleteEvidenceFileUseCase)
    {
        _config = configuration;
        _logger = logger;
        _parentGateway = parentGateway;
        _checkGateway = checkGateway;
        _loadParentDetailsUseCase = loadParentDetailsUseCase;
        _performEligibilityCheckUseCase = performEligibilityCheckUseCase;
        _enterChildDetailsUseCase = enterChildDetailsUseCase;
        _processChildDetailsUseCase = processChildDetailsUseCase;
        _getCheckStatusUseCase = getCheckStatusUseCase;
        _addChildUseCase = addChildUseCase;
        _removeChildUseCase = removeChildUseCase;
        _changeChildDetailsUseCase = changeChildDetailsUseCase;
        _createUserUseCase = createUserUseCase;
        _submitApplicationUseCase = submitApplicationUseCase;
        _validateParentDetailsUseCase = validateParentDetailsUseCase;
        _uploadEvidenceFileUseCase = uploadEvidenceFileUseCase;
        _validateEvidenceFileUse = validateEvidenceFileUseCase;
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _deleteEvidenceFileUseCase = deleteEvidenceFileUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Consent_Declaration()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Consent_Declaration_Approval(string consent)
    {
        if (consent == "checked") return RedirectToAction("Enter_Details");

        return View("Consent_Declaration", true);
    }

    [HttpGet]
    public async Task<IActionResult> Enter_Details()
    {
        var (parent, validationErrors) = await _loadParentDetailsUseCase.Execute(
            TempData["ParentDetails"]?.ToString(),
            TempData["Errors"]?.ToString()
        );

        if (validationErrors != null)
            foreach (var (key, errorList) in validationErrors)
                foreach (var error in errorList)
                    ModelState.AddModelError(key, error);

        return View(parent);
    }

    [HttpPost]
    public async Task<IActionResult> Enter_Details(ParentGuardian request)
    {
        var validationResult = _validateParentDetailsUseCase.Execute(request, ModelState);

        if (!validationResult.IsValid)
        {
            TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
            TempData["Errors"] = JsonConvert.SerializeObject(validationResult.Errors);
            return RedirectToAction("Enter_Details");
        }

        // Clear data when starting a new application
        TempData.Remove("FsmApplication");
        TempData.Remove("FsmEvidence");

        var response = await _performEligibilityCheckUseCase.Execute(request, HttpContext.Session);
        TempData["Response"] = JsonConvert.SerializeObject(response);

        return RedirectToAction("Loader");
    }

    public async Task<IActionResult> Loader()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        var responseJson = TempData["Response"] as string;
        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

            if (outcome == "queuedForProcessing")
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;

            _logger.LogError(outcome);

            var isLA = _Claims?.Organisation?.Category?.Name == Constants.CategoryTypeLA; //false=school
            switch (outcome)
            {
                case "eligible":
                    return View(isLA ? "Outcome/Eligible_LA" : "Outcome/Eligible");
                    break;

                case "notEligible":
                    return View(isLA ? "Outcome/Not_Eligible_LA" : "Outcome/Not_Eligible");
                    break;

                case "parentNotFound":
                    return View("Outcome/Not_Found");
                    break;

                case "queuedForProcessing":
                    return View("Loader");
                    break;

                default:
                    return View("Outcome/Technical_Error");
            }
        }
        catch (Exception ex)
        {
            return View("Outcome/Technical_Error");
        }
    }


    [HttpGet]
    public IActionResult Enter_Child_Details()
    {
        var childrenModel = _enterChildDetailsUseCase.Execute(
            TempData["ChildList"] as string,
            TempData["IsChildAddOrRemove"] as bool?);


        return View(childrenModel);
    }

    [HttpPost]
    public IActionResult Enter_Child_Details(Children request)
    {
        if (TempData["FsmApplication"] != null && TempData["IsRedirect"] != null && (bool)TempData["IsRedirect"])
            return View("Enter_Child_Details", request);

        if (!ModelState.IsValid) return View("Enter_Child_Details", request);

        var fsmApplication = _processChildDetailsUseCase.Execute(request, HttpContext.Session).Result;
        if (HttpContext.Session.GetString("CheckResult")=="eligible")
        {
            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            return RedirectToAction("Check_Answers");
        }
        // Restore evidence from TempData if it exists (from ChangeChildDetails)
        if (TempData["FsmEvidence"] != null)
        {
            var savedEvidence = JsonConvert.DeserializeObject<Evidence>(TempData["FsmEvidence"].ToString());
            fsmApplication.Evidence = savedEvidence;

            TempData.Remove("FsmEvidence");
        }
        else
        {
            fsmApplication.Evidence = new Evidence { EvidenceList = new List<EvidenceFile>() };
        }

        TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

        return RedirectToAction("UploadEvidence");
    }

    [HttpPost]
    public IActionResult Add_Child(Children request)
    {
        try
        {
            TempData["IsChildAddOrRemove"] = true;

            var updatedChildren = _addChildUseCase.Execute(request);

            TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);
        }
        catch (MaxChildrenException e)
        {
            TempData["ChildList"] = JsonConvert.SerializeObject(request.ChildList);
        }

        return RedirectToAction("Enter_Child_Details");
    }

    [HttpPost]
    public async Task<IActionResult> Remove_Child(Children request, int index)
    {
        try
        {
            TempData["IsChildAddOrRemove"] = true;

            var updatedChildren = await _removeChildUseCase.Execute(request, index);

            TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);

            return RedirectToAction("Enter_Child_Details");
        }

        catch (RemoveChildValidationException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return RedirectToAction("Enter_Child_Details");
        }
    }

    [HttpGet]
    public IActionResult Check_Answers()
    {
        if (TempData["FsmApplication"] != null)
        {
            var fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            // Re-save the application data to TempData for the next request
            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
            return View("Check_Answers", fsmApplication);
        }

        // Fallback - empty model
        return View("Check_Answers");
    }
    //
    [HttpPost]
    [ActionName("Check_Answers")]
    public async Task<IActionResult> Check_Answers_Post(FsmApplication request)
    {
        if (TempData["FsmApplication"] != null)
        {
            var savedApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            if (savedApplication.Evidence?.EvidenceList?.Count > 0)
            {
                request.Evidence = savedApplication.Evidence;
            }
        }

        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
        var userId = await _createUserUseCase.Execute(HttpContext.User.Claims);

        var responses = await _submitApplicationUseCase.Execute(
            request,
            userId,
            _Claims.Organisation.Urn);

        TempData["FsmApplicationResponse"] = JsonConvert.SerializeObject(responses);

        foreach(var response in responses)
        {
            try
            {
                var notificationRequest = new NotificationRequest
                {
                    Data = new NotificationRequestData
                    {
                        Email = response.Data.ParentEmail,
                        Type = NotificationType.ParentApplicationSuccessful,
                        Personalisation = new Dictionary<string, object>
                        {
                        { "reference", $"{response.Data.Reference}" },
                        { "parentFirstName", $"{request.ParentFirstName}" }
                    }
                    }
                };

                await _sendNotificationUseCase.Execute(notificationRequest);
                _logger.LogInformation("Notification sent successfully for application reference: {Reference}",
                    response.Data.Reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for application reference: {Reference}",
                    response.Data.Reference);
            }
        }

        return RedirectToAction(
            responses.FirstOrDefault()?.Data.Status == "Entitled"
                ? "ApplicationsRegistered"
                : "AppealsRegistered");
    }

    [HttpPost]
    public IActionResult RemoveEvidenceItem(string fileName, string redirectAction)
    {
        if (TempData["FsmApplication"] != null)
        {
            var fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            var evidenceItem = fsmApplication.Evidence.EvidenceList.FirstOrDefault(e => e.FileName == fileName);
            if (evidenceItem != null)
            {
                fsmApplication.Evidence.EvidenceList.Remove(evidenceItem);
                TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
            }

            // Delete the file from blob storage
            if (evidenceItem != null && !string.IsNullOrEmpty(evidenceItem.StorageAccountReference))
            {
                _deleteEvidenceFileUseCase.Execute(evidenceItem.StorageAccountReference, _config["AzureStorageEvidence:EvidenceFilesContainerName"]);
            }
        }

        return RedirectToAction(redirectAction);
    }

    public IActionResult ChangeChildDetails(int child)
    {
        TempData["IsRedirect"] = true;
        var model = new Children { ChildList = new List<Child>() };
        var fsmApplication = new FsmApplication();

        try
        {
            if (TempData["FsmApplication"] != null)
            {
                fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());

                // Save the evidence
                TempData["FsmEvidence"] = JsonConvert.SerializeObject(fsmApplication.Evidence);
            }

            model = _changeChildDetailsUseCase.Execute(
                TempData["FsmApplication"] as string);
        }
        catch (JSONException e)
        {
            ;
        }
        catch (NoChildException)
        {
            ;
        }

        return View("Enter_Child_Details", model);
    }


    [HttpGet]
    public IActionResult ApplicationsRegistered()
    {
        var vm = JsonConvert.DeserializeObject<List<ApplicationSaveItemResponse>>(TempData["FsmApplicationResponse"]
            .ToString());
        return View("ApplicationsRegistered", vm);
    }


    [HttpGet]
    public IActionResult AppealsRegistered()
    {
        var vm = JsonConvert.DeserializeObject<List<ApplicationSaveItemResponse>>(TempData["FsmApplicationResponse"]
            .ToString());
        return View("AppealsRegistered", vm);
    }

    [HttpGet]
    public IActionResult UploadEvidence()
    {
        if (TempData["FsmApplication"] != null)
        {
            var fsmApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
            return View(fsmApplication);
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadEvidence(FsmApplication request)
    {
        ModelState.Clear();
        var isValid = true;

        try
        {
            var updatedRequest = new FsmApplication
            {
                ParentFirstName = request.ParentFirstName,
                ParentLastName = request.ParentLastName,
                ParentNino = request.ParentNino,
                ParentNass = request.ParentNass ?? string.Empty, // Ensure not null
                ParentDateOfBirth = request.ParentDateOfBirth,
                ParentEmail = request.ParentEmail,
                Children = request.Children,
                Evidence = new Evidence { EvidenceList = new List<EvidenceFile>() }
            };

            // Retrieve existing application with evidence from TempData
            if (TempData["FsmApplication"] != null)
            {
                var existingApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());

                // Add existing evidence files if they exist
                if (existingApplication?.Evidence?.EvidenceList != null && existingApplication.Evidence.EvidenceList.Any())
                {
                    updatedRequest.Evidence.EvidenceList.AddRange(existingApplication.Evidence.EvidenceList);
                }
            }

            //Handle no evidence files selected
            if (request.EvidenceFiles == null && updatedRequest.Evidence.EvidenceList.Count == 0)
            {
                ModelState.AddModelError("EvidenceFiles", $"You have not selected a file");
                TempData["ErrorMessage"] = "You have not selected a file";
            }

            // Process new files from the form if any were uploaded
            if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
            {
                foreach (var file in request.EvidenceFiles)
                {
                    var validationResult = _validateEvidenceFileUse.Execute(file);
                    if (!validationResult.IsValid)
                    {
                        isValid = false;
                        TempData["ErrorMessage"] = validationResult.ErrorMessage;

                        continue;
                    }

                    try
                    {
                        if (file.Length > 0)
                        {
                            string blobUrl = await _uploadEvidenceFileUseCase.Execute(file, _config["AzureStorageEvidence:EvidenceFilesContainerName"]);

                            updatedRequest.Evidence.EvidenceList.Add(new EvidenceFile
                            {
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                StorageAccountReference = blobUrl
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload evidence file {FileName}", file.FileName);
                        ModelState.AddModelError("EvidenceFiles", $"Failed to upload file {file.FileName}");
                    }
                }
            }

            // preserve any evidence files that came from the form submission
            if (request.Evidence?.EvidenceList != null && request.Evidence.EvidenceList.Any())
            {
                var existingFiles = updatedRequest.Evidence.EvidenceList
                    .Select(f => f.StorageAccountReference)
                    .ToHashSet();

                foreach (var file in request.Evidence.EvidenceList)
                {
                    // Only add files that aren't already in our list
                    if (!string.IsNullOrEmpty(file.StorageAccountReference) &&
                        !existingFiles.Contains(file.StorageAccountReference))
                    {
                        updatedRequest.Evidence.EvidenceList.Add(file);
                        existingFiles.Add(file.StorageAccountReference);
                    }
                }
            }

            TempData["FsmApplication"] = JsonConvert.SerializeObject(updatedRequest);

            if (!ModelState.IsValid || !isValid)
            {
                return View("UploadEvidence", updatedRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There has been an error, please try again");
            ModelState.AddModelError("EvidenceFiles", $"There has been an error, please try again");

            return View("UploadEvidence");

        }
        return RedirectToAction("Check_Answers");
    }

    [HttpPost]
    public IActionResult ContinueWithoutMoreFiles(FsmApplication request)
    {
        var application = new FsmApplication
        {
            ParentFirstName = request.ParentFirstName,
            ParentLastName = request.ParentLastName,
            ParentNino = request.ParentNino,
            ParentNass = request.ParentNass,
            ParentDateOfBirth = request.ParentDateOfBirth,
            ParentEmail = request.ParentEmail,
            Children = request.Children,
            Evidence = request.Evidence
        };

        TempData["FsmApplication"] = JsonConvert.SerializeObject(application);

        return RedirectToAction("Check_Answers");
    }
}