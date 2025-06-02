using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Domain.Enums;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.UseCases;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;
using Child = CheckYourEligibility.FrontEnd.Models.Child;

namespace CheckYourEligibility.FrontEnd.Controllers;

public class CheckController : Controller
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
    private readonly IParentGateway _parentGatewayService;
    private readonly IPerformEligibilityCheckUseCase _performEligibilityCheckUseCase;
    private readonly IProcessChildDetailsUseCase _processChildDetailsUseCase;
    private readonly IRemoveChildUseCase _removeChildUseCase;
    private readonly ISearchSchoolsUseCase _searchSchoolsUseCase;
    private readonly ISignInUseCase _signInUseCase;
    private readonly ISubmitApplicationUseCase _submitApplicationUseCase;
    private readonly IUploadEvidenceFileUseCase _uploadEvidenceFileUseCase;
    private readonly ISendNotificationUseCase _sendNotificationUseCase;
    private readonly IDeleteEvidenceFileUseCase _deleteEvidenceFileUseCase;
    private readonly IValidateEvidenceFileUseCase _validateEvidenceFileUseCase;

    public CheckController(
        ILogger<CheckController> logger,
        IParentGateway ecsParentGatewayService,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        ISearchSchoolsUseCase searchSchoolsUseCase,
        ILoadParentDetailsUseCase loadParentDetailsUseCase,
        ICreateUserUseCase createUserUseCase,
        IPerformEligibilityCheckUseCase performEligibilityCheckUseCase,
        IGetCheckStatusUseCase getCheckStatusUseCase,
        ISignInUseCase signInUseCase,
        IEnterChildDetailsUseCase enterChildDetailsUseCase,
        IProcessChildDetailsUseCase processChildDetailsUseCase,
        IAddChildUseCase addChildUseCase,
        IRemoveChildUseCase removeChildUseCase,
        ISubmitApplicationUseCase submitApplicationUseCase,
        IChangeChildDetailsUseCase changeChildDetailsUseCase,
        ISendNotificationUseCase sendNotificationUseCase,
        IUploadEvidenceFileUseCase uploadEvidenceFileUseCase,
        IDeleteEvidenceFileUseCase deleteEvidenceFileUseCase,
        IValidateEvidenceFileUseCase validateEvidenceFileUseCase)


    {
        _config = configuration;
        _logger = logger;
        _parentGatewayService = ecsParentGatewayService;
        _checkGateway = checkGateway;
        _searchSchoolsUseCase = searchSchoolsUseCase;
        _createUserUseCase = createUserUseCase;
        _loadParentDetailsUseCase = loadParentDetailsUseCase;
        _performEligibilityCheckUseCase = performEligibilityCheckUseCase;
        _getCheckStatusUseCase = getCheckStatusUseCase;
        _signInUseCase = signInUseCase;
        _enterChildDetailsUseCase = enterChildDetailsUseCase;
        _processChildDetailsUseCase = processChildDetailsUseCase;
        _addChildUseCase = addChildUseCase;
        _removeChildUseCase = removeChildUseCase;
        _submitApplicationUseCase = submitApplicationUseCase;
        _changeChildDetailsUseCase = changeChildDetailsUseCase;
        _uploadEvidenceFileUseCase = uploadEvidenceFileUseCase;
        _deleteEvidenceFileUseCase = deleteEvidenceFileUseCase;
        _validateEvidenceFileUseCase = validateEvidenceFileUseCase;
        _sendNotificationUseCase = sendNotificationUseCase;

        _logger.LogInformation("controller log info");
    }

    [HttpGet]
    public async Task<IActionResult> Enter_Details()
    {
        var viewModel = await _loadParentDetailsUseCase.Execute(
            TempData["ParentDetails"]?.ToString(),
            TempData["Errors"]?.ToString()
        );

        if (viewModel.ValidationErrors != null)
            foreach (var (key, errorList) in viewModel.ValidationErrors)
                foreach (var error in errorList)
                    ModelState.AddModelError(key, error);

        return View(viewModel.Parent);
    }

    [HttpPost]
    public async Task<IActionResult> Enter_Details(Parent request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
            TempData["Errors"] = JsonConvert.SerializeObject(errors);

            return RedirectToAction("Enter_Details");
        }

        var (response, responseCode) = await _performEligibilityCheckUseCase.Execute(request, HttpContext.Session);

        switch (responseCode)
        {
            case "Success":
                TempData["Response"] = JsonConvert.SerializeObject(response);
                return RedirectToAction("Loader");

            case "Nass":
                TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
                return RedirectToAction("Nass");

            default:
                return View("Outcome/Could_Not_Check");
        }
    }


    public IActionResult Nass()
    {
        var parentDetailsJson = TempData["ParentDetails"] as string;
        if (string.IsNullOrEmpty(parentDetailsJson)) return RedirectToAction("Enter_Details");

        var parent = JsonConvert.DeserializeObject<Parent>(parentDetailsJson) ?? new Parent();

        return View(parent);
    }


    public async Task<IActionResult> Loader()
    {
        var responseJson = TempData["Response"] as string;
        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

            if (outcome == "queuedForProcessing")
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;

            _logger.LogError(outcome);

            switch (outcome)
            {
                case "eligible":
                    return View("Outcome/Eligible");
                    break;

                case "notEligible":
                    return View("Outcome/Not_Eligible");
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

    public async Task<IActionResult> SignIn()
    {
        var properties = await _signInUseCase.Execute("/Check/CreateUser");
        return Challenge(properties, OneLoginDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> CreateUser()
    {
        try
        {
            var email = HttpContext.User.Claims.First(c => c.Type == "email").Value;
            var uniqueId = HttpContext.User.Claims.First(c => c.Type == "sub").Value;

            var userId = await _createUserUseCase.Execute(email, uniqueId);

            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("UserId", userId);

            return RedirectToAction("Enter_Child_Details");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return View("Outcome/Technical_Error");
        }
    }

    public async Task<IActionResult> Enter_Child_Details()
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
        if (HttpContext.Session.GetString("CheckResult") == "eligible")
        {
            TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            return RedirectToAction("Check_Answers");
        }
        // Restore evidence from TempData if it exists (from ChangeChildDetails)
        if (TempData["FsmEvidence"] != null)
        {
            var savedEvidence = JsonConvert.DeserializeObject<Evidences>(TempData["FsmEvidence"].ToString());
            fsmApplication.Evidence = savedEvidence;

            TempData.Remove("FsmEvidence");
        }
        else
        {
            fsmApplication.Evidence = new Evidences { EvidenceList = new List<EvidenceFile>() };
        }

        TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

        return RedirectToAction("Upload_Evidence_Type");
    }

    [HttpPost]
    public async Task<IActionResult> Add_Child(Children request)
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

            var updatedChildren = _removeChildUseCase.Execute(request, index);

            TempData["ChildList"] = JsonConvert.SerializeObject(updatedChildren.ChildList);

            return RedirectToAction("Enter_Child_Details");
        }

        catch (RemoveChildValidationException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return RedirectToAction("Enter_Child_Details");
        }
    }

    /// this method is called by AJAX
    [HttpGet]
    public async Task<IActionResult> SearchSchools(string query)
    {
        try
        {
            // Sanitize input before processing
            var sanitizedQuery = query?.Trim()
                .Replace(Environment.NewLine, "")
                .Replace("\n", "")
                .Replace("\r", "")
                // Add more sanitization as needed
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

            if (string.IsNullOrEmpty(sanitizedQuery) || sanitizedQuery.Length < 3)
            {
                _logger.LogWarning("Invalid school search query: {Query}", sanitizedQuery);
                return BadRequest("Query must be at least 3 characters long.");
            }

            var schools = await _searchSchoolsUseCase.Execute(sanitizedQuery);
            return Json(schools.ToList());
        }
        catch (Exception ex)
        {
            // Log sanitized query only
            _logger.LogError(ex, "Error searching schools for query: {Query}",
                query?.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", ""));
            return BadRequest("An error occurred while searching for schools.");
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
            ViewData["eligibility"] = HttpContext.Session.GetString("CheckResult");
            return View("Check_Answers", fsmApplication);
        }

        // Fallback - empty model
        return View("Check_Answers");
    }

    [HttpPost]
    [ActionName("Check_Answers")]
    public async Task<IActionResult> Check_Answers_Post(FsmApplication request, string finishedConfirmation)
    {
        if (HttpContext.Session.GetString("CheckResult") == "notEligible")
        {
            ViewData["eligibility"] = HttpContext.Session.GetString("CheckResult");

            if (finishedConfirmation != "finishedConfirmationChecked")
            {
                TempData["ValidationMessage"] = "You must confirm that you have finished adding children or evidence to this application";
                return View("Check_Answers", request);
            }
        }

        if (TempData["FsmApplication"] != null)
        {
            var savedApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            if (savedApplication.Evidence?.EvidenceList?.Count > 0)
            {
                request.Evidence = savedApplication.Evidence;
            }
        }

        var currentStatus = HttpContext.Session.GetString("CheckResult");
        var userId = HttpContext.Session.GetString("UserId");
        var email = HttpContext.Session.GetString("Email");

        var responses = await _submitApplicationUseCase.Execute(
            request, currentStatus, userId, email);

        // for each response send a notification
        foreach (var response in responses)
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

        TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(responses);
        return RedirectToAction("Application_Sent");
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

    [HttpGet]
    public async Task<IActionResult> Application_Sent()
    {
        ModelState.Clear();
        return View("Application_Sent");
    }

    public async Task<IActionResult> ChangeChildDetails(int child)
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
    public IActionResult Upload_Evidence_Type()
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
    public IActionResult Upload_Evidence_Type(FsmApplication request, string evidenceType)
    {
        if (evidenceType == "digital")
        {
            return RedirectToAction("Upload_Guidance_Digital");
        }
        else if (evidenceType == "paper")
        {
            return RedirectToAction("Upload_Guidance_Paper");
        }
        else if (evidenceType == "none")
        {
            return RedirectToAction("Check_Answers");
        }

        return BadRequest("You must select one option");
    }

    [HttpGet]
    public IActionResult Upload_Guidance_Digital()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Upload_Guidance_Paper()
    {
        return View();
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
                Email = request.Email,
                Children = request.Children,
                Evidence = new Evidences { EvidenceList = new List<EvidenceFile>() }
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
            if (request.EvidenceFiles == null || request.EvidenceFiles.Count == 0)
            {
                ModelState.AddModelError("EvidenceFiles", $"You have not selected a file");
                TempData["ErrorMessage"] = "You have not selected a file";
            }

            // Process new files from the form if any were uploaded
            if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
            {
                foreach (var file in request.EvidenceFiles)
                {
                    var validationResult = _validateEvidenceFileUseCase.Execute(file);
                    if (!validationResult.IsValid)
                    {
                        isValid = false;
                        ModelState.AddModelError("EvidenceFiles", $"Failed to upload file {file.FileName}");
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
            Email = request.Email,
            Children = request.Children,
            Evidence = request.Evidence
        };

        TempData["FsmApplication"] = JsonConvert.SerializeObject(application);

        return RedirectToAction("Check_Answers");
    }
}