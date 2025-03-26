using System.Diagnostics;
using CheckYourEligibility.Admin.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<CheckController> _logger;

    public ErrorController(ILogger<CheckController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        // Get the details of the exception that occurred
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult NotFound()
    {
        return View();
    }
}