using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.FrontEnd.Controllers;

[AutoValidateAntiforgeryToken]
public class HomeController : Controller
{
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;

    private readonly ILogger<CheckController> _logger;
    private readonly IParentGateway _parentGatewayService;
    private IParentGateway _object;

    public HomeController(ILogger<CheckController> logger, IParentGateway ecsParentGatewayService,
        ICheckGateway checkGateway, IConfiguration configuration)
    {
        _config = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parentGatewayService =
            ecsParentGatewayService ?? throw new ArgumentNullException(nameof(ecsParentGatewayService));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));

        _logger.LogInformation("controller log info");
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Accessibility()
    {
        return View();
    }

    public IActionResult Cookies()
    {
        return View();
    }

    public IActionResult fsm_print_version()
    {
        return View();
    }

    public IActionResult Parental_Guidance()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SchoolList()
    {
        var viewModel = new SchoolListViewModel();
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult SchoolList(SchoolListViewModel viewModel)
    {
        if (string.IsNullOrEmpty(viewModel.SelectedSchoolURN))
        {
            ModelState.AddModelError("SelectedSchoolURN", "Select a school");
            return View(viewModel);
        }

        TempData["SchoolName"] = viewModel.SelectedSchoolName;
        TempData["SchoolLA"] = viewModel.SelectedSchoolLA;
        TempData["SchoolPostcode"] = viewModel.SelectedSchoolPostcode;

        if (viewModel.SelectedSchoolInPrivateBeta == true)
        {
            return RedirectToAction("SchoolInPrivateBeta");
        }

        return RedirectToAction("SchoolNotInPrivateBeta");
    }

    public IActionResult SchoolInPrivateBeta()
    {
        // Set session flag to indicate user has completed private beta check
        HttpContext.Session.SetString("PrivateBetaConfirmed", "true");

        ViewData["SchoolName"] = TempData["SchoolName"];
        ViewData["SchoolLA"] = TempData["SchoolLA"];
        ViewData["SchoolPostcode"] = TempData["SchoolPostcode"];
        return View();
    }

    public IActionResult SchoolNotInPrivateBeta()
    {
        // Clear private beta session flag if user selects a non-private beta school
        HttpContext.Session.Remove("PrivateBetaConfirmed");

        ViewData["SchoolName"] = TempData["SchoolName"];
        ViewData["SchoolLA"] = TempData["SchoolLA"];
        ViewData["SchoolPostcode"] = TempData["SchoolPostcode"];
        return View();
    }
}