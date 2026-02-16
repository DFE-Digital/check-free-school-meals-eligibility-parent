using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using static CheckYourEligibility.Admin.Models.Constants;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CheckYourEligibility.Admin.Controllers;

public class BulkCheckFsmBasicController : BaseController
{
    private const int TotalErrorsToDisplay = 20;
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;
    private readonly IParseBulkCheckFileUseCase_FsmBasic _parseBulkCheckFileUseCase;
    private readonly IGetBulkCheckStatusesUseCase_FsmBasic _getBulkCheckStatusesUseCase;
    private readonly IDeleteBulkCheckFileUseCase_FsmBasic _deleteBulkCheckFileUseCase;
    private readonly ILogger<BulkCheckFsmBasicController> _logger;

    public BulkCheckFsmBasicController(
        ILogger<BulkCheckFsmBasicController> logger,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        IParseBulkCheckFileUseCase_FsmBasic parseBulkCheckFileUseCase,
        IGetBulkCheckStatusesUseCase_FsmBasic getBulkCheckStatusesUseCase,
        IDeleteBulkCheckFileUseCase_FsmBasic deleteBulkCheckFileUseCase)
    {
        _config = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
        _parseBulkCheckFileUseCase = parseBulkCheckFileUseCase ?? throw new ArgumentNullException(nameof(parseBulkCheckFileUseCase));
        _getBulkCheckStatusesUseCase = getBulkCheckStatusesUseCase ?? throw new ArgumentNullException(nameof(getBulkCheckStatusesUseCase));
        _deleteBulkCheckFileUseCase = deleteBulkCheckFileUseCase ?? throw new ArgumentNullException(nameof(deleteBulkCheckFileUseCase));
    }

    // GET: Upload page
    public IActionResult Bulk_Check_FSMB()
    {
        return View();
    }

    // POST: Handle file upload
    [HttpPost]
    public async Task<IActionResult> Bulk_Check_FSMB(IFormFile fileUpload)
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);

        // Validate file
        if (fileUpload == null)
        {
            TempData["ErrorMessage"] = "Select a CSV file";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        if (fileUpload.Length >= 10 * 1024 * 1024) // 10MB limit
        {
            TempData["ErrorMessage"] = "File size must be less than 10MB";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        if (fileUpload.ContentType.ToLower() != "text/csv")
        {
            TempData["ErrorMessage"] = "Please upload a CSV file";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        // Rate limiting check
        var timeNow = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FirstSubmissionTimeStamp")))
        {
            var firstSubmissionTimeStampString = HttpContext.Session.GetString("FirstSubmissionTimeStamp");
            DateTime.TryParse(firstSubmissionTimeStampString, out var firstSubmissionTimeStamp);
            var timein1Hour = firstSubmissionTimeStamp.AddHours(1);

            if (timeNow >= timein1Hour)
            {
                HttpContext.Session.Remove("BulkSubmissions");
            }
        }

        var sessionCount = 0;
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("BulkSubmissions")))
        {
            HttpContext.Session.SetInt32("BulkSubmissions", 0);
            HttpContext.Session.SetString("FirstSubmissionTimeStamp", DateTime.UtcNow.ToString());
        }
        else
        {
            sessionCount = HttpContext.Session.GetInt32("BulkSubmissions") ?? 0;
        }

        sessionCount++;
        HttpContext.Session.SetInt32("BulkSubmissions", sessionCount);

        if (sessionCount > int.Parse(_config["BulkUploadAttemptLimit"] ?? "5"))
        {
            TempData["ErrorMessage"] = "You have exceeded the maximum number of bulk upload attempts. Please try again later.";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        // Parse CSV file
        BulkCheckCsvResultFsmBasic parseResult;
        try
        {
            using var stream = fileUpload.OpenReadStream();
            parseResult = await _parseBulkCheckFileUseCase.Execute(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV file");
            TempData["ErrorMessage"] = "Error reading the CSV file. Please check the file format.";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        // Handle parsing errors
        if (!string.IsNullOrEmpty(parseResult.ErrorMessage))
        {
            TempData["ErrorMessage"] = parseResult.ErrorMessage;
            return RedirectToAction("Bulk_Check_FSMB");
        }

        if (parseResult.Errors.Any())
        {
            var errorsViewModel = new BulkCheckFsmBasicErrorsViewModel
            {
                Response = "data_issue",
                ErrorMessage = $"The file contains {parseResult.Errors.Count} error(s). Please correct them and try again.",
                Errors = parseResult.Errors.Take(TotalErrorsToDisplay).Select(e => new CheckRowErrorFsmBasic
                {
                    LineNumber = e.LineNumber,
                    Message = e.Message
                }),
                TotalErrorCount = parseResult.Errors.Count
            };

            return View("BulkOutcomeFsmBasic/Error_Data_Issue_FSMB", errorsViewModel);
        }

        if (!parseResult.ValidRequests.Any())
        {
            TempData["ErrorMessage"] = "The file contains no valid records.";
            return RedirectToAction("Bulk_Check_FSMB");
        }

        // Submit bulk check
        try
        {
            // Get LocalAuthorityId if user is from a Local Authority
            string? localAuthorityId = null;
            if (_Claims?.Organisation?.Category?.Name == CategoryTypeLA)
            {
                localAuthorityId = _Claims.Organisation.EstablishmentNumber;
            }

            var bulkCheckRequest = new CheckEligibilityRequestBulk_FsmBasic
            {
                Data = parseResult.ValidRequests,
                Meta = new CheckEligibilityRequestBulkMeta
                {
                    Filename = fileUpload.FileName,
                    SubmittedBy = _Claims?.User.FirstName + " " + _Claims?.User.Surname ?? "",
                    LocalAuthorityId = localAuthorityId
                }
            };

            var response = await _checkGateway.PostBulkCheck_FsmBasic(bulkCheckRequest);

            if (response?.Links?.Get_BulkCheck_Status != null)
            {
                HttpContext.Session.SetString("BulkCheckUrl", response.Links.Get_BulkCheck_Status);

                var fileSubmittedViewModel = new BulkCheckFsmBasicFileSubmittedViewModel
                {
                    Filename = fileUpload.FileName,
                    NumberOfRecords = parseResult.ValidRequests.Count
                };
                return RedirectToAction("Bulk_Check_History_FSMB");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to submit bulk check. Please try again.";
                return RedirectToAction("Bulk_Check_FSMB");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting bulk check");
            TempData["ErrorMessage"] = "An error occurred while submitting the bulk check. Please try again.";
            return RedirectToAction("Bulk_Check_FSMB");
        }
    }

    // GET: Check bulk check progress
    public async Task<IActionResult> Bulk_Check_Status_FSMB(string? bulkCheckId = null)
    {
        try
        {
            string? bulkCheckUrl = bulkCheckId != null
                ? $"bulk-check/{bulkCheckId}/status"
                : HttpContext.Session.GetString("BulkCheckUrl");

            if (string.IsNullOrEmpty(bulkCheckUrl))
            {
                return RedirectToAction("Bulk_Check_History");
            }

            var result = await _checkGateway.GetBulkCheckProgress_FsmBasic(bulkCheckUrl);

            if (result != null)
            {
                // If complete, redirect to status page showing table
                if (result.Data.Complete >= result.Data.Total)
                {
                    // Extract bulkCheckId from URL if not provided
                    if (string.IsNullOrEmpty(bulkCheckId))
                    {
                        var urlParts = bulkCheckUrl.Split('/');
                        if (urlParts.Length > 1)
                        {
                            bulkCheckId = urlParts[^2]; // Get second to last part
                        }
                    }

                    HttpContext.Session.Remove("BulkCheckUrl");
                    return RedirectToAction("Bulk_Check_Complete", new { bulkCheckId });
                }

                // Still processing - show progress
                ViewBag.Total = result.Data.Total;
                ViewBag.Complete = result.Data.Complete;
                ViewBag.BulkCheckUrl = bulkCheckUrl;

                return View("Bulk_Check_Processing");
            }

            return RedirectToAction("Bulk_Check_History");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bulk check status");
            return RedirectToAction("Bulk_Check_History");
        }
    }

    // GET: Show completion message and link to history
    public IActionResult Bulk_Check_Complete_FSMB(string bulkCheckId)
    {
        ViewBag.BulkCheckId = bulkCheckId;
        return View();
    }

    // GET: Batch checks history with table
    public async Task<IActionResult> Bulk_Check_History_FSMB(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
            var organisationId = _Claims?.Organisation?.EstablishmentNumber ?? string.Empty;

            if (string.IsNullOrEmpty(organisationId))
            {
                _logger.LogWarning("No organisation ID found for user");
                return View("Bulk_Check_History", new BulkCheckFsmBasicStatusesViewModel());
            }

            var allChecks = await _getBulkCheckStatusesUseCase.Execute(organisationId);

            var checksList = allChecks
                .Where(c => c.Status != "Deleted")
                .ToList();

            // Sort by date descending
            checksList = checksList.OrderByDescending(x => x.SubmittedDate).ToList();

            // Pagination
            var totalRecords = checksList.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedChecks = checksList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new BulkCheckFsmBasicStatusesViewModel
            {
                Checks = pagedChecks.Select(c => new BulkCheckFsmBasicStatusViewModel
                {
                    BulkCheckId = c.BulkCheckId,
                    Filename = c.Filename,
                    NumberOfRecords = c.NumberOfRecords,
                    FinalNameInCheck = c.FinalNameInCheck,
                    DateSubmitted = c.SubmittedDate,
                    SubmittedBy = c.SubmittedBy,
                    Status = c.Status
                }).ToList(),
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return View("BulkOutcomeFsmBasic/Bulk_Check_History_FSMB", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bulk check history");
            return View("BulkOutcomeFsmBasic/Bulk_Check_History_FSMB", new BulkCheckFsmBasicStatusesViewModel());
        }
    }

    // GET: View results for a specific bulk check
    public async Task<IActionResult> Bulk_Check_View_Results_FSMB(string bulkCheckId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bulkCheckId))
            {
                return RedirectToAction("Bulk_Check_History");
            }

            var resultsUrl = $"bulk-check/{bulkCheckId}/results";
            var results = await _checkGateway.GetBulkCheckResults_FsmBasic(resultsUrl);

            if (results?.Data == null || !results.Data.Any())
            {
                TempData["ErrorMessage"] = "No results found for this bulk check.";
                return RedirectToAction("Bulk_Check_History");
            }

            // Show results in a view or download
            ViewBag.BulkCheckId = bulkCheckId;
            ViewBag.Results = results.Data;

            return View(results);
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, "Error viewing bulk check results for ID: {BulkCheckId}", safeBulkCheckId);
            TempData["ErrorMessage"] = "Error loading results.";
            return RedirectToAction("Bulk_Check_History");
        }
    }

    // GET: Download results as CSV
    public async Task<IActionResult> Bulk_Check_Download_FSMB(string bulkCheckId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bulkCheckId))
            {
                return RedirectToAction("Bulk_Check_History");
            }

            var results = await _checkGateway.LoadBulkCheckResults_FsmBasic(bulkCheckId);

            if (results == null || !results.Any())
            {
                TempData["ErrorMessage"] = "No results found for this bulk check.";
                return RedirectToAction("Bulk_Check_History");
            }

            // Generate CSV
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(results.Cast<BulkExport>());
            }

            var fileName = $"fsm-basic-outcomes-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(memoryStream.ToArray(), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, "Error downloading bulk check results for ID: {BulkCheckId}", safeBulkCheckId);
            TempData["ErrorMessage"] = "Error downloading results.";
            return RedirectToAction("Bulk_Check_History");
        }
    }

    public async Task<IActionResult> Bulk_Check_Delete_FSMB(string bulkCheckId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bulkCheckId))
            {
                TempData["ErrorMessage"] = "Invalid bulk check ID.";
                return RedirectToAction("Bulk_Check_History");
            }

            var response = await _deleteBulkCheckFileUseCase.Execute(bulkCheckId);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Batch check deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message ?? "Failed to delete bulk check.";
            }

            return RedirectToAction("Bulk_Check_History_FSMB");
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, "Error deleting bulk check: {BulkCheckId}", safeBulkCheckId);
            TempData["ErrorMessage"] = "Error deleting bulk check.";
            return RedirectToAction("Bulk_Check_History_FSMB");
        }
    }
}
