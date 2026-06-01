using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using static CheckYourEligibility.Admin.ViewModels.ReportHistoryViewModel;

namespace CheckYourEligibility.Admin.Controllers;

[FeatureGate("Reports")]
public class EligibilityCheckReportingController : BaseController
{
    private readonly ILogger<EligibilityCheckReportingController> _logger;
    private readonly IGenerateEligibilityCheckReportUseCase _generateEligibilityCheckReportUseCase;
    private readonly IDeleteEligibilityCheckReportUseCase _deleteEligibilityCheckReportUseCase;
    private readonly IEligibilityCheckReportingGateway _eligibilityCheckReportingGateway;


    public EligibilityCheckReportingController(
    ILogger<EligibilityCheckReportingController> logger,
    IEligibilityCheckReportingGateway eligibilityCheckReportingGateway,
    IGenerateEligibilityCheckReportUseCase generateEligibilityCheckReportUseCase,
    IDeleteEligibilityCheckReportUseCase deleteEligibilityCheckReportUseCase,
    IDfeSignInApiService dfeSignInApiService,
    ISchoolMenuContextResolver schoolMenuContextResolver,
    ILocalAuthoritySettingsGateway localAuthoritySettingsGateway) : base(dfeSignInApiService, schoolMenuContextResolver, localAuthoritySettingsGateway)
    {
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
                CheckType = model.CheckType

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

    [HttpGet]
    public async Task<IActionResult> Download_Report(string reportId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reportId))
            {
                return RedirectToAction("Reports");
            }

            var response = await _eligibilityCheckReportingGateway
                .GetEligibilityCheckReportItems(reportId);

            if (response?.Data == null || !response.Data.Any())
            {
                TempData["ErrorMessage"] = "No report data found.";
                return RedirectToAction("Reports");
            }

            using var memoryStream = new MemoryStream();

            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var exportData = response.Data.Select(x => new EligibilityCheckReportCsvExport
                {
                    ParentSurname = x.ParentName,
                    NationalInsuranceNumber = x.NationalInsuranceNumber,
                    DateOfBirth = x.DateOfBirth.ToString("d MMMM yyyy"),
                    DateCheckSubmitted = x.DateCheckSubmitted.ToString("d MMMM yyyy"),
                    CheckType = x.ProcessingType,
                    CheckedBy = x.CheckedBy,
                    Outcome = x.OutcomeDisplay
                }).ToList();

                csv.WriteRecords(exportData);
            }

            var fileName = $"eligibility-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(memoryStream.ToArray(), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            var sanitizedReportId = reportId?
             .Replace("\r", " ")
             .Replace("\n", " ")
             .Replace("\t", " ");

            _logger.LogError(ex, "Error downloading report for reportId: {ReportId}", sanitizedReportId);

            TempData["ErrorMessage"] = "Error downloading report.";

            return RedirectToAction("Reports");
        }
    }
}


