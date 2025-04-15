using Azure.Core;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    private readonly IBlobStorageGateway _blobStorageGateway;
    private const string EvidenceContainerName = "content";


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
        IBlobStorageGateway blobStorageGateway)
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
        _blobStorageGateway = blobStorageGateway ?? throw new ArgumentNullException(nameof(blobStorageGateway));
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

    public IActionResult Check_Answers()
    {
        return View("Check_Answers");
    }

    [HttpPost]
    public async Task<IActionResult> Check_Answers(FsmApplication request)
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

        return RedirectToAction(
            responses.FirstOrDefault()?.Data.Status == "Entitled"
                ? "ApplicationsRegistered"
                : "AppealsRegistered");
    }


    public IActionResult ChangeChildDetails(int child)
    {
        TempData["IsRedirect"] = true;
        var model = new Children { ChildList = new List<Child>() };

        try
        {
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
            return View(fsmApplication);
        }
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UploadEvidence(FsmApplication request)
    {
            if (TempData["FsmApplication"] != null)
        {
            var existingApplication = JsonConvert.DeserializeObject<FsmApplication>(TempData["FsmApplication"].ToString());
            
            if (existingApplication.Evidence?.EvidenceList != null && existingApplication.Evidence.EvidenceList.Any())
            {
                if (request.Evidence == null)
                    request.Evidence = new Evidence { EvidenceList = new List<EvidenceFile>() };
                    
                request.Evidence.EvidenceList.AddRange(existingApplication.Evidence.EvidenceList);
            }
        }
        
        if (request.Evidence == null)
            request.Evidence = new Evidence { EvidenceList = new List<EvidenceFile>() };
            
        if (request.EvidenceFiles != null && request.EvidenceFiles.Count > 0)
        {
            foreach (var file in request.EvidenceFiles)
            {
                try
                {
                    // Upload file to Azure Blob Storage
                    string blobUrl = await _blobStorageGateway.UploadFileAsync(file, EvidenceContainerName);
                    
                    request.Evidence.EvidenceList.Add(new EvidenceFile
                    {
                        FileName = file.FileName,
                        FileType = file.ContentType,
                        StorageAccountReference = blobUrl
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload evidence file {FileName}", file.FileName);
                    ModelState.AddModelError("EvidenceFiles", $"Failed to upload file {file.FileName}");
                }
            }
        }
        
        TempData["FsmApplication"] = JsonConvert.SerializeObject(request);
        return View("Check_Answers", request);
    }
}