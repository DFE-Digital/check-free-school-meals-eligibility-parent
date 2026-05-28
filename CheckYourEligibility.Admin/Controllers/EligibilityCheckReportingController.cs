using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using static CheckYourEligibility.Admin.ViewModels.ReportHistoryViewModel;

namespace CheckYourEligibility.Admin.Controllers;

[FeatureGate("Reports")]
public class EligibilityCheckReportingController : BaseController
{
    private readonly IConfiguration _config;
    private readonly ILogger<EligibilityCheckReportingController> _logger;
    private readonly IGenerateEligibilityCheckReportUseCase _generateEligibilityCheckReportUseCase;
    private readonly IDeleteEligibilityCheckReportUseCase _deleteEligibilityCheckReportUseCase;
    private readonly IEligibilityCheckReportingGateway _eligibilityCheckReportingGateway;


    public EligibilityCheckReportingController(
    ILogger<EligibilityCheckReportingController> logger,
    IParentGateway parentGateway,
    IEligibilityCheckReportingGateway eligibilityCheckReportingGateway,
    IConfiguration configuration,
    IGenerateEligibilityCheckReportUseCase generateEligibilityCheckReportUseCase,
    IDeleteEligibilityCheckReportUseCase deleteEligibilityCheckReportUseCase,
    IDfeSignInApiService dfeSignInApiService,
    ISchoolMenuContextResolver schoolMenuContextResolver) : base(dfeSignInApiService, schoolMenuContextResolver)
    {
        _config = configuration;
        _logger = logger;
        _eligibilityCheckReportingGateway = eligibilityCheckReportingGateway;
        _deleteEligibilityCheckReportUseCase = deleteEligibilityCheckReportUseCase;
        _generateEligibilityCheckReportUseCase = generateEligibilityCheckReportUseCase;
    }


    [HttpGet]

    public async Task<IActionResult> Reports(int PageNumber = 1)
    {
       
        try
        {
            var claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            var localAuthorityId = claims.Organisation.EstablishmentNumber;
            var history = await _eligibilityCheckReportingGateway.GetEligibilityCheckReportHistory(localAuthorityId, PageNumber);
            var viewModel = new ReportHistoryViewModel
            {
                PageNumber = history.PageNumber,
                PageSize = history.PageSize,
                TotalNumberOfRecords = history.TotalNumberOfRecords,
                Data = history.Data.Select(x => new ReportHistoryItemViewModel
                {
                    Item = x,
                    StatusBanner = new StatusBanner(x.Status)
                })
            };
            return View("~/Views/Check/Report/Report_History.cshtml", viewModel);
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

        return View("~/Views/Check/Report/Create_Report.cshtml", model);
    }

    [HttpGet]
    public IActionResult View_Historical_Report(DateTime startDate, DateTime endDate, DateTime generated)
    {
        var request = new EligibilityCheckReportRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            LocalAuthorityID = Convert.ToInt32(_Claims.Organisation.EstablishmentNumber),
            GeneratedBy = _Claims.User.FirstName,
            SaveRequestAudit = false,
            CheckType = CheckType.BulkChecks

        };
        HttpContext.Session.SetString("StartDateDisplay", startDate.ToString("d MMMM yyyy"));
        HttpContext.Session.SetString("EndDateDisplay", endDate.ToString("d MMMM yyyy"));
        HttpContext.Session.SetString("ReportGeneratedDate", generated.ToString("yyyy-MM-dd"));
        TempData["ReportRequest"] = JsonConvert.SerializeObject(request);

        return RedirectToAction("Report_Loader");
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
                StartDate = model.StartDateValue,
                EndDate = model.EndDateValue,
                LocalAuthorityID = Convert.ToInt32(_Claims.Organisation.EstablishmentNumber),
                GeneratedBy = $"{_Claims.User.FirstName ?? ""} {_Claims.User.Surname ?? ""}".Trim(),
                SaveRequestAudit = true,
                CheckType = CheckType.BulkChecks

            };
            var response = await _generateEligibilityCheckReportUseCase.Execute(request);
            return RedirectToAction("Reports");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report");
            return View("Outcome/Technical_Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Report_Loader()
    {
        if (HttpContext.Session.GetString("ReportStarted") == null)
        {
            HttpContext.Session.SetString("ReportStarted", "true");
            TempData.Keep("ReportRequest");
            return View("Report/Report_Loader");
        }

        TempData.Keep("ReportRequest");

        var reqJson = TempData["ReportRequest"] as string;
        var request = JsonConvert.DeserializeObject<EligibilityCheckReportRequest>(reqJson);

        try
        {
            var response = await _generateEligibilityCheckReportUseCase.Execute(request);
            HttpContext.Session.SetString("FullReportData", JsonConvert.SerializeObject(response));
            HttpContext.Session.Remove("ReportStarted");

            TempData.Keep("ReportRequest");
            TempData.Keep("StartDateDisplay");
            TempData.Keep("EndDateDisplay");

            return RedirectToAction("Report_Results", new { pageNumber = 1 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report");
            return View("Outcome/Technical_Error");
        }
    }
    [HttpGet]
    public IActionResult Delete_Report_Confirmation(string reportID, DateTime reportGeneratedDate, string generatedBy)
    {
        try
        {
          
            if (string.IsNullOrEmpty(reportID))
            {
                return RedirectToAction("Reports");
            }
            var viewModel = new DeleteReportConfirmationViewModel
            {
                ReportID = reportID,
                GeneratedBy = generatedBy,
                GeneratedDate = reportGeneratedDate
            };

            return View("~/Views/Check/Report/Delete_Report_Confirmation.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load delete confirmation page");
            return View("Outcome/Technical_Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete_Report(string reportId)
    {
        try
        {
            await _deleteEligibilityCheckReportUseCase.Execute(reportId);
            return RedirectToAction("Reports");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete report");
            return View("Outcome/Technical_Error");
        }
    }
}


