using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Domain.Enums;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.UseCases;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    private readonly ISendNotificationUseCase _sendNotificationUseCase;

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
        ISendNotificationUseCase sendNotificationUseCase)

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
    public async Task<IActionResult> Enter_Child_Details(Children request)
    {
        if (!ModelState.IsValid) return View(request);

        var isRedirect = TempData["FsmApplication"] != null &&
                         TempData["IsRedirect"] != null &&
                         (bool)TempData["IsRedirect"];

        var validationErrors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());

        if (isRedirect) return View("Enter_Child_Details", request);

        try
        {
            var model = await _processChildDetailsUseCase.Execute(
                request,
                HttpContext.Session,
                validationErrors);

            if (model is FsmApplication fsmApplication)
                TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
            return View("Check_Answers", model);
        }

        catch (ProcessChildDetailsUseCase.ProcessChildDetailsValidationException e)
        {
            Dictionary<string, string[]>
                errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(e.Message);
            foreach (var error in errors)
                foreach (var message in error.Value)
                    ModelState.AddModelError(error.Key, message);

            return View("Enter_Child_Details");
        }
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


    public IActionResult Check_Answers()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Check_Answers(FsmApplication request)
    {
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

    [HttpGet]
    public async Task<IActionResult> Application_Sent()
    {
        ModelState.Clear();
        return View("Application_Sent");
    }

    public async Task<IActionResult> ChangeChildDetails()
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
}