using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.UseCases;
using CheckYourEligibility.Admin.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading.Tasks;
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
    private readonly ISearchSchoolsUseCase _searchSchoolsUseCase;
    private readonly ISubmitApplicationUseCase _submitApplicationUseCase;
    private readonly IValidateParentDetailsUseCase _validateParentDetailsUseCase;
    private readonly IUploadEvidenceFileUseCase _uploadEvidenceFileUseCase;
    private readonly IValidateEvidenceFileUseCase _validateEvidenceFileUse;
    private readonly ISendNotificationUseCase _sendNotificationUseCase;
    private readonly IDeleteEvidenceFileUseCase _deleteEvidenceFileUseCase;
    private readonly IGenerateEligibilityCheckReportUseCase _generateEligibilityCheckReportUseCase;


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
        ISearchSchoolsUseCase searchSchoolsUseCase,
        IChangeChildDetailsUseCase changeChildDetailsUseCase,
        ICreateUserUseCase createUserUseCase,
        ISubmitApplicationUseCase submitApplicationUseCase,
        IValidateParentDetailsUseCase validateParentDetailsUseCase,
        IUploadEvidenceFileUseCase uploadEvidenceFileUseCase,
        IValidateEvidenceFileUseCase validateEvidenceFileUseCase,
        ISendNotificationUseCase sendNotificationUseCase,
        IDeleteEvidenceFileUseCase deleteEvidenceFileUseCase,
        IGenerateEligibilityCheckReportUseCase generateEligibilityCheckReportUseCase,
        IDfeSignInApiService dfeSignInApiService) : base(dfeSignInApiService)
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
        _searchSchoolsUseCase = searchSchoolsUseCase;
        _changeChildDetailsUseCase = changeChildDetailsUseCase;
        _createUserUseCase = createUserUseCase;
        _submitApplicationUseCase = submitApplicationUseCase;
        _validateParentDetailsUseCase = validateParentDetailsUseCase;
        _uploadEvidenceFileUseCase = uploadEvidenceFileUseCase;
        _validateEvidenceFileUse = validateEvidenceFileUseCase;
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _deleteEvidenceFileUseCase = deleteEvidenceFileUseCase;
        _generateEligibilityCheckReportUseCase = generateEligibilityCheckReportUseCase;
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
        
        if (_Claims.Roles.Any().Equals("Basic")) {
            return RedirectToAction("Enter_Details_Basic");
        }

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
    [HttpGet]
    public async Task<IActionResult> Enter_Details_Basic()
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

        return RedirectToAction("Loader", request);
    }
    [HttpPost]
    public async Task<IActionResult> Enter_Details_Basic(ParentGuardianBasic request)
    {
        var validationResult = _validateParentDetailsUseCase.ExecuteBasic(request, ModelState);

        if (!validationResult.IsValid)
        {
            TempData["ParentDetails"] = JsonConvert.SerializeObject(request);
            TempData["Errors"] = JsonConvert.SerializeObject(validationResult.Errors);
            return RedirectToAction("Enter_Details_Basic");
        }

        // Clear data when starting a new application
        TempData.Remove("FsmApplication");
        TempData.Remove("FsmEvidence");

        var response = await _performEligibilityCheckUseCase.ExecuteBasic(request, HttpContext.Session);
        TempData["Response"] = JsonConvert.SerializeObject(response);

        return RedirectToAction("Loader_Basic", request);
    }
    public async Task<IActionResult> Loader(ParentGuardian request)
    {
        if (TempData["ParentGuardianRequest"] != null) // Means it was queued previously and stored in temp
        {
            var json = TempData["ParentGuardianRequest"] as string;
            request = JsonConvert.DeserializeObject<ParentGuardian>(json);
        }

        var responseJson = TempData["Response"] as string;
        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

            if (outcome == "queuedForProcessing")
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;

            _logger.LogError(outcome);

            OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
            TempData["organisationType"] = organisationType;

            switch (outcome)
            {
                case "eligible":
                    switch (organisationType)
                    {
                        case OrganisationCategory.LocalAuthority:
                            return View("Outcome/Eligible", request);
                        case OrganisationCategory.MultiAcademyTrust:
                            return View("Outcome/Eligible_LA", request);
                        case OrganisationCategory.Establishment: //school
                            return View("Outcome/Eligible", request);
                        default:
                            return View("Outcome/Technical_Error");
                    }
                case "notEligible":
                    switch (organisationType)
                    {
                        case OrganisationCategory.LocalAuthority:
                            return View("Outcome/Not_Eligible_LA", request);
                        case OrganisationCategory.MultiAcademyTrust:
                            return View("Outcome/Not_Eligible_LA", request);
                        case OrganisationCategory.Establishment: //school:
                            return View("Outcome/Not_Eligible", request);
                        default:
                            return View("Outcome/Technical_Error");
                    }
                case "parentNotFound":
                    return View("Outcome/Not_Found");
                case "queuedForProcessing":
                    TempData["ParentGuardianRequest"] = JsonConvert.SerializeObject(request);
                    return View("Loader");
                default:
                    return View("Outcome/Technical_Error");
            }
        }
        catch (Exception ex)
        {
            return View("Outcome/Technical_Error");
        }
    }
    public async Task<IActionResult> Loader_Basic(ParentGuardianBasic request)
    {
        if (TempData["ParentGuardianRequest"] != null) // Means it was queued previously and stored in temp
        {
            var json = TempData["ParentGuardianRequest"] as string;
            request = JsonConvert.DeserializeObject<ParentGuardianBasic>(json);
        }

        var responseJson = TempData["Response"] as string;
        try
        {
            var outcome = await _getCheckStatusUseCase.Execute(responseJson, HttpContext.Session);

            if (outcome == "queuedForProcessing")
                // Save the response back to TempData for the next poll
                TempData["Response"] = responseJson;

            _logger.LogError(outcome);

            OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
            TempData["organisationType"] = organisationType;

            switch (outcome)
            {
                case "eligible":
                    switch (organisationType)
                    {
                        case OrganisationCategory.LocalAuthority:
                            return View("Outcome/Eligible_Basic", request);
                        case OrganisationCategory.MultiAcademyTrust:
                            return View("Outcome/Eligible_LA", request);
                        case OrganisationCategory.Establishment: //school
                            return View("Outcome/Eligible", request);
                        default:
                            return View("Outcome/Technical_Error");
                    }
                case "notEligible":
                    switch (organisationType)
                    {
                        case OrganisationCategory.LocalAuthority:
                            return View("Outcome/Not_Eligible_LA", request);
                        case OrganisationCategory.MultiAcademyTrust:
                            return View("Outcome/Not_Eligible_LA", request);
                        case OrganisationCategory.Establishment: //school:
                            return View("Outcome/Not_Eligible", request);
                        default:
                            return View("Outcome/Technical_Error");
                    }
                case "parentNotFound":
                    return View("Outcome/Not_Found");
                case "queuedForProcessing":
                    TempData["ParentGuardianRequest"] = JsonConvert.SerializeObject(request);
                    return View("Loader_Basic");
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

        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

        return View(childrenModel);
    }

    [HttpPost]
    public IActionResult Enter_Child_Details(Children request)
    {
        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

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
            string organisationType;
            string organisationNumber;
            if (_Claims.Organisation.Category.Id == OrganisationCategory.MultiAcademyTrust)
            {
                organisationType = "mat";
                organisationNumber = _Claims.Organisation.Uid;
            }
            else
            {
                organisationType = "la";
                organisationNumber = _Claims.Organisation.EstablishmentNumber;
            }
            var schools = await _searchSchoolsUseCase.Execute(sanitizedQuery, organisationNumber, organisationType);
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

            OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
            TempData["organisationType"] = organisationType;

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

        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

        // var userId = await _createUserUseCase.Execute(HttpContext.User.Claims);

        var responses = await _submitApplicationUseCase.Execute(
            request,
            null,
            _Claims.Organisation.Urn);

        TempData["FsmApplicationResponse"] = JsonConvert.SerializeObject(responses);

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

        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

        return View("Enter_Child_Details", model);
    }


    [HttpGet]
    public IActionResult ApplicationsRegistered()
    {
        var vm = JsonConvert.DeserializeObject<List<ApplicationSaveItemResponse>>(TempData["FsmApplicationResponse"]
            .ToString());

        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

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
    public async Task<IActionResult> UploadEvidence(FsmApplication request, string actionType)
    {
        ModelState.Clear();
        var isValid = true;

        var evidenceExists = false;

        if (string.Equals(actionType, "email"))
        {
            evidenceExists = true;
        }

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
                evidenceExists = true;
            }
        }

        if ((request.EvidenceFiles == null || !request.EvidenceFiles.Any()) && !evidenceExists)
        {
            isValid = false;
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
    [HttpGet]
    public async Task<IActionResult> Reports()
    {
        try
        {
            var claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            var localAuthorityId = claims.Organisation.EstablishmentNumber;
            var history = await _checkGateway.GetEligibilityCheckReportHistory(localAuthorityId);
            return View("Report/report-history", history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve report history");
            return View("Outcome/Technical_Error");
        }
    }
    public IActionResult Create_Report()
    {
        var model = new EligibilityCheckReportViewModel();

        if (TempData.ContainsKey("ReportForm"))
        {
            model = JsonConvert.DeserializeObject<EligibilityCheckReportViewModel>(
                TempData["ReportForm"].ToString()
            );
        }

        if (TempData.ContainsKey("Errors"))
        {
            var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(
                TempData["Errors"].ToString()
            );

            foreach (var kvp in errors)
            {
                foreach (var msg in kvp.Value)
                {
                    ModelState.AddModelError(kvp.Key, msg);
                }
            }
        }

        return View("Report/Create_Report", model);
    }

    [HttpGet]
    public IActionResult View_Historical_Report(DateTime startDate, DateTime endDate)
    {
        var request = new EligibilityCheckReportRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            LocalAuthorityID = Convert.ToInt32(_Claims.Organisation.EstablishmentNumber),
            GeneratedBy = _Claims.User.FirstName,
            CheckType = CheckType.BulkChecks
        };

        TempData["ReportRequest"] = JsonConvert.SerializeObject(request);

        return RedirectToAction("Report_Loader");
    }
    [HttpGet]
    public async Task<IActionResult> Report_Results(int pageNumber = 1, int pageSize = 100)
    {
        if (!TempData.ContainsKey("ReportRequest"))
        {
            return RedirectToAction("Create_Report");
        }

        TempData.Keep("ReportRequest");
        var reqJson = TempData["ReportRequest"] as string;
        var request = JsonConvert.DeserializeObject<EligibilityCheckReportRequest>(reqJson);

        var fullResponse = await _generateEligibilityCheckReportUseCase.Execute(request);

        var totalRecords = fullResponse.Data.Count();
        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        var pagedData = fullResponse.Data
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        fullResponse.Data = pagedData;

        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = totalPages;
        ViewBag.RecordsPerPage = pageSize;
        ViewBag.TotalRecords = totalRecords;
        var paginationModel = new PaginationPartialViewModel
        {
            CurrentPage = pageNumber,
            TotalPages = totalPages,
            RecordsPerPage = pageSize,
            TotalRecords = totalRecords,
            ControllerName = "Report_Results",
            Keyword = null,
            Status = null,
            DateFrom = null
        };

        ViewBag.PaginationModel = paginationModel;

        return View("Report/Report_Results", fullResponse);
    }
    [HttpPost]
    public async Task<IActionResult> Create_Report(EligibilityCheckReportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errorDict = ModelState
                .Where(kvp => kvp.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            TempData["Errors"] = JsonConvert.SerializeObject(errorDict);
            TempData["ReportForm"] = JsonConvert.SerializeObject(model);

            return RedirectToAction("Create_Report");
        }

        try
        {
            var request = new EligibilityCheckReportRequest
            {
                StartDate = model.StartDateValue.Value,
                EndDate = model.EndDateValue.Value,
                LocalAuthorityID = Convert.ToInt32(_Claims.Organisation.EstablishmentNumber),
                GeneratedBy = _Claims.User.FirstName,
                CheckType = CheckType.BulkChecks
            };

            TempData["ReportRequest"] = JsonConvert.SerializeObject(request);
            return RedirectToAction("Report_Loader");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report");
            return View("Outcome/Technical_Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Report_Loader(EligibilityCheckReportRequest request)
    {
        if (!TempData.ContainsKey("ReportStarted"))
        {
            TempData["ReportStarted"] = true;
            TempData.Keep("ReportRequest");
            return View("Report/Report_Loader");
        }

        TempData.Keep("ReportRequest");
        var reqJson = TempData["ReportRequest"] as string;
        request = JsonConvert.DeserializeObject<EligibilityCheckReportRequest>(reqJson);

        try
        {
            var response = await _generateEligibilityCheckReportUseCase.Execute(request);

            TempData.Remove("ReportStarted");
            // keep ReportRequest so we can reuse it for pagination
            TempData.Keep("ReportRequest");

            // instead of returning the view directly:
            return RedirectToAction("Report_Results", new { pageNumber = 1 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report");
            return View("Outcome/Technical_Error");
        }
    }

    [HttpPost]
    public IActionResult Report_Download(string jsonModel)
    {
        if (string.IsNullOrEmpty(jsonModel))
            return RedirectToAction("Create_Report");

        var response = JsonConvert.DeserializeObject<EligibilityCheckReportResponse>(jsonModel);

        var exportData = response.Data.Select(x => new ReportExport
        {
            ParentName = x.ParentName,
            NationalInsuranceNumber = x.NationalInsuranceNumber,
            DateOfBirth = x.DateOfBirth.ToString("d MMM yyyy"),
            DateCheckSubmitted = x.DateCheckSubmitted.ToString("d MMM yyyy"),
            CheckType = x.CheckType.ToString(),
            CheckedBy = x.CheckedBy
        });

        var fileName = $"eligibility-check-report-{DateTime.Now:yyyyMMdd}.csv";

        var result = WriteCsvToMemory(exportData);
        var memoryStream = new MemoryStream(result);

        return new FileStreamResult(memoryStream, "text/csv")
        {
            FileDownloadName = fileName
        };
    }


    private byte[] WriteCsvToMemory(IEnumerable<object> records)
    {
        using (var memoryStream = new MemoryStream())
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteRecords(records);
            streamWriter.Flush();
            return memoryStream.ToArray();
        }
    }

}