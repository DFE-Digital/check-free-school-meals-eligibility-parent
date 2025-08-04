using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.FrontEnd.Controllers;

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
    public async Task<IActionResult> SchoolList()
    {
        var schoolList = await _parentGatewayService.GetSchool("school");

        // Check if schoolList or schoolList.Data is null
        var schools = schoolList?.Data?.ToList() ?? new List<Establishment>();

        var viewModel = new SchoolListViewModel
        {
            Schools = schools
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SchoolList(SchoolListViewModel viewModel)
    {
        if (viewModel.IsRadioSelected.HasValue)
        {
            if (viewModel.IsRadioSelected == true) return RedirectToAction("Enter_Details", "Check");

            return Redirect("https://www.gov.uk/apply-free-school-meals"); // this could be in appsettings instead
        }

        var schoolList = await _parentGatewayService.GetSchool("school");
        var schools = schoolList?.Data?.ToList() ?? new List<Establishment>();

        viewModel.Schools = schools;
        return View(viewModel);
    }
}