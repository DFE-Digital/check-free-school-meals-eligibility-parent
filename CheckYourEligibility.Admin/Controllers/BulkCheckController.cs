using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.CsvMaps;
using CheckYourEligibility.Admin.Domain.Constants;
using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Globalization;
using System.Text;
using static CheckYourEligibility.Admin.Domain.Constants.DfeSignInRoles;
using static CheckYourEligibility.Admin.Helpers.CsvBulkCheckValidatorHelper;

namespace CheckYourEligibility.Admin.Controllers;

public class BulkCheckController : BaseController
{
    private const int TotalErrorsToDisplay = 20;
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;
    private readonly IParseBulkCheckFileUseCase _parseBulkCheckFileUseCase;
    private readonly IGetBulkChecks _getBulkCheckStatusesUseCase;
    private readonly IDeleteBulkCheckFileUseCase _deleteBulkCheckFileUseCase;
    private readonly ILogger<BulkCheckController> _logger;
    private readonly IWebHostEnvironment _environment;
    private (int id, OrganisationCategory type) _organisation =>
        GetOrganisationIdandType();

    public BulkCheckController(
        ILogger<BulkCheckController> logger,
        ICheckGateway checkGateway,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IParseBulkCheckFileUseCase parseBulkCheckFileUseCase,
        IGetBulkChecks getBulkCheckStatusesUseCase,
        IDeleteBulkCheckFileUseCase deleteBulkCheckFileUseCase,
        IDfeSignInApiService dfeSignInApiService,
        ISchoolMenuContextResolver schoolMenuContextResolver,
        ILocalAuthoritySettingsGateway localAuthoritySettingsGateway
        ) : base(dfeSignInApiService, schoolMenuContextResolver, localAuthoritySettingsGateway)
    {
        _config = configuration;
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
        _parseBulkCheckFileUseCase = parseBulkCheckFileUseCase ?? throw new ArgumentNullException(nameof(parseBulkCheckFileUseCase));
        _getBulkCheckStatusesUseCase = getBulkCheckStatusesUseCase ?? throw new ArgumentNullException(nameof(getBulkCheckStatusesUseCase));
        _deleteBulkCheckFileUseCase = deleteBulkCheckFileUseCase ?? throw new ArgumentNullException(nameof(deleteBulkCheckFileUseCase));

    }

    // GET: Upload page
    public IActionResult Bulk_Check()
    {
        var role = _Claims.Roles[0].Code;
        var org = _Claims.Organisation.Category.Id;

        switch (role)
        {
            case DfeSignInRoles.RoleCodeBasic:
                var viewModel = new BulkCheckUploadViewModel
                {
                    isSchool = org == OrganisationCategory.Establishment ? true : false,
                    isEnhanced = false,
                    GuidanceItems = BulkCheckConstants.GuidanceItemsBasic
                };
                return View("Bulk_Check", viewModel);
            default:

                bool isSchool = org == OrganisationCategory.Establishment ? true : false;
                var viewModelEnhanced = new BulkCheckUploadViewModel
                {
                    isSchool = isSchool,
                    isEnhanced = true,
                    GuidanceItems = BulkCheckConstants.GuidanceItemsEnhanced(isSchool)
                };


                return View("Bulk_Check", viewModelEnhanced);
        }

    }

    [HttpGet]
    public IActionResult DownloadTemplate(bool isEnhanced, bool isSchool)
    {
        string fileName = string.Empty;
        switch (isEnhanced, isSchool)
        {
            case (true, true):
                fileName = "BulkCheckTemplate_Enhanced_School.csv";
                break;
            case (true, false):
                fileName = "BulkCheckTemplate_Enhanced.csv";
                break;
            default:
                fileName = "BulkCheckTemplate.csv";
                break;
        }


        var path = Path.Combine(_environment.WebRootPath, "documents", fileName);

        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        return PhysicalFile(path, "text/csv", fileName);
    }
    [HttpPost]
    public async Task<IActionResult> Bulk_Check(IFormFile fileUpload, BulkCheckUploadViewModel model)
    {
        if (fileUpload == null)
        {
            TempData["ErrorMessage"] = "Select a CSV file";
            return RedirectToAction("Bulk_Check");
        }
        if (fileUpload.Length >= 10 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "File size must be less than 10MB";
            return RedirectToAction("Bulk_Check");
        }
        if (fileUpload.ContentType.ToLower() != "text/csv")
        {
            TempData["ErrorMessage"] = "Please upload a CSV file";
            return RedirectToAction("Bulk_Check");
        }

        // Rate limiting
        var timeNow = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FirstSubmissionTimeStamp")))
        {
            var firstSubmissionTimeStampString = HttpContext.Session.GetString("FirstSubmissionTimeStamp");
            DateTime.TryParse(firstSubmissionTimeStampString, out var firstSubmissionTimeStamp);
            var timein1Hour = firstSubmissionTimeStamp.AddHours(1);
            if (timeNow >= timein1Hour) HttpContext.Session.Remove("BulkSubmissions");
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
            return RedirectToAction("Bulk_Check");
        }

        try
        {
            using var stream = fileUpload.OpenReadStream();

            switch (model.isEnhanced, model.isSchool)
            {
                case (true, true):
                    {
                        var parseResult = await _parseBulkCheckFileUseCase.Execute<CheckEligibilityRequestData_Enhanced>(
                            stream,
                            CreateEnhancedSchoolRequestItem,
                            BulkCheckConstants.enhancedSchoolHeaders, _organisation.id, _organisation.type, schoolUrn: _Claims.Organisation.Urn);

                        var result = ValidateParseResult(parseResult, fileUpload.FileName);
                        if (result != null) return result;

                        var bulkReq = new CheckEligibilityRequestBulk_Enhanced
                        {
                            Data = parseResult.ValidRequests,
                            Meta = new CheckEligibilityRequestBulkMeta
                            {
                                Filename = fileUpload.FileName,
                                SubmittedBy = _Claims?.User.FirstName + " " + _Claims?.User.Surname ?? "",
                                LocalAuthorityId = _Claims?.Organisation?.Category?.Name == CategoryTypeLA ? _Claims.Organisation.EstablishmentNumber : null
                            }
                        };

                        return await SubmitAndHandleResponseAsync(
                            bulkReq,
                            _checkGateway.PostBulkCheck,
                            parseResult.ValidRequests.Count,
                            fileUpload.FileName);
                    }
                case (true, false):
                    {
                        var parseResult = await _parseBulkCheckFileUseCase.Execute<CheckEligibilityRequestData_Enhanced>(
                            stream,
                            CreateEnhancedRequestItem,
                            BulkCheckConstants.enhancedHeaders, _organisation.id, _organisation.type);

                        var result = ValidateParseResult(parseResult, fileUpload.FileName);
                        if (result != null) return result;

                        var bulkReq = new CheckEligibilityRequestBulk_Enhanced
                        {
                            Data = parseResult.ValidRequests,
                            Meta = new CheckEligibilityRequestBulkMeta
                            {
                                Filename = fileUpload.FileName,
                                SubmittedBy = _Claims?.User.FirstName + " " + _Claims?.User.Surname ?? "",
                                LocalAuthorityId = _Claims?.Organisation?.Category?.Name == CategoryTypeLA ? _Claims.Organisation.EstablishmentNumber : null
                            }
                        };

                        return await SubmitAndHandleResponseAsync(
                            bulkReq,
                            _checkGateway.PostBulkCheck,
                            parseResult.ValidRequests.Count,
                            fileUpload.FileName);
                    }

                default:
                    {
                        var parseResult = await _parseBulkCheckFileUseCase.Execute<CheckEligibilityRequestDataBase>(
                            stream,
                            CreateRequestItem,
                            BulkCheckConstants.Headers, _organisation.id, _organisation.type);

                        var result = ValidateParseResult(parseResult, fileUpload.FileName);
                        if (result != null) return result;

                        var bulkReq = new CheckEligibilityRequestBulk
                        {
                            Data = parseResult.ValidRequests,
                            Meta = new CheckEligibilityRequestBulkMeta
                            {
                                Filename = fileUpload.FileName,
                                SubmittedBy = _Claims?.User.FirstName + " " + _Claims?.User.Surname ?? "",
                                LocalAuthorityId = _Claims?.Organisation?.Category?.Name == CategoryTypeLA ? _Claims.Organisation.EstablishmentNumber : null
                            }
                        };

                        return await SubmitAndHandleResponseAsync(
                            bulkReq,
                            _checkGateway.PostBulkCheck,
                            parseResult.ValidRequests.Count,
                            fileUpload.FileName);
                    }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV file");
            TempData["ErrorMessage"] = "Error reading the CSV file. Please check the file format.";
            return RedirectToAction("Bulk_Check");
        }
    }
    // GET: Show completion message and link to history
    public IActionResult Bulk_Check_Complete(string bulkCheckId)
    {
        ViewBag.BulkCheckId = bulkCheckId;
        return View();
    }

    // GET: Batch checks history with table
    public async Task<IActionResult> Bulk_Check_History(int pageNumber = 1, int pageSize = 10)
    {

        try
        {
            if (_organisation.id == 0)
            {
                _logger.LogWarning("No organisation ID found for user");
                return View(new BulkCheckViewModel());
            }

            var allChecks = await _getBulkCheckStatusesUseCase.Execute(_organisation.id);

            // Pagination
            var totalRecords = allChecks.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedChecks = allChecks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new BulkCheckViewModel
            {
                Checks = pagedChecks.Select(c => new BulkCheckStatusViewModel
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

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bulk check history");
            return View(new BulkCheckViewModel());
        }
    }

    // GET: View results for a specific bulk check
    public async Task<IActionResult> Bulk_Check_View_Results(string bulkCheckId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(bulkCheckId))
            {
                return RedirectToAction("Bulk_Check_History");
            }

            var resultsUrl = $"bulk-check/{bulkCheckId}/results";
            var results = await _checkGateway.GetBulkCheckResults(resultsUrl);

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

    public async Task<IActionResult> Bulk_Check_Download(string bulkCheckId)
    {
        try
        {
            var results = Enumerable.Empty<IBulkExport>();
            bool isEnhanced = _Claims.Roles[0].Code == DfeSignInRoles.RoleCodeBasic ? false : true;
            var fsmPolicy = await GetFreeSchoolMealsPolicy();

            if (string.IsNullOrWhiteSpace(bulkCheckId))
            {
                return RedirectToAction("Bulk_Check_History");
            }

            results = isEnhanced
            ? await _checkGateway.LoadBulkCheckResults<BulkExport>(bulkCheckId)
            : await _checkGateway.LoadBulkCheckResults<BulkExportBase>(bulkCheckId);

            if (results == null || !results.Any())
            {
                TempData["ErrorMessage"] = "No results found for this bulk check.";
                return RedirectToAction("Bulk_Check_History");
            }

            // Generate CSV
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(
                memoryStream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                WriteBulkExportRecords(csv, results, isEnhanced, fsmPolicy.EligibilityCriteria);

            }
            var fileName = $"fsm-outcomes-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
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

    public async Task<IActionResult> Bulk_Check_Delete(string bulkCheckId)
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

            return RedirectToAction("Bulk_Check_History");
        }
        catch (Exception ex)
        {
            var safeBulkCheckId = bulkCheckId?.Replace("\r", "").Replace("\n", "");
            _logger.LogError(ex, "Error deleting bulk check: {BulkCheckId}", safeBulkCheckId);
            TempData["ErrorMessage"] = "Error deleting bulk check.";
            return RedirectToAction("Bulk_Check_History");
        }
    }

    private void WriteBulkExportRecords(CsvWriter csv,IEnumerable results, bool isEnhanced,EligibilityCriteria eligibilityCriteria)
    {
       
        // Select correct map
        var mapType = GetCsvMapType(isEnhanced, eligibilityCriteria);

        csv.Context.RegisterClassMap(mapType);
        csv.WriteRecords(results);
    }

    private Type GetCsvMapType(bool isEnhanced, EligibilityCriteria eligibilityCriteria)
    {
        if (isEnhanced)
        {
            return eligibilityCriteria == EligibilityCriteria.expanded
                ? typeof(BulkExportExpandedCsvMap)
                : typeof(BulkExportCsvMap);
        }

        return eligibilityCriteria == EligibilityCriteria.expanded
            ? typeof(BulkExportBaseExpandedCsvMap)
            : typeof(BulkExportBaseCsvMap);
    }

    private IActionResult ValidateParseResult<T>(BulkCheckCsvResult<T> parseResult, string filename) where T : CheckEligibilityRequestDataBase
    {
        if (!string.IsNullOrEmpty(parseResult.ErrorMessage))
        {
            TempData["ErrorMessage"] = parseResult.ErrorMessage;
            return RedirectToAction("Bulk_Check");
        }

        if (parseResult.Errors.Any())
        {
            var errorsViewModel = new BulkCheckErrorsViewModel
            {
                Response = "data_issue",
                ErrorMessage = $"The file contains {parseResult.Errors.Count} error(s). Please correct them and try again.",
                Errors = parseResult.Errors.Take(TotalErrorsToDisplay).Select(e => new CsvRowError
                {
                    LineNumber = e.LineNumber,
                    Message = e.Message
                }),
                TotalErrorCount = parseResult.Errors.Count
            };

            return View("Error_Data_Issue", errorsViewModel);
        }

        if (!parseResult.ValidRequests.Any())
        {
            TempData["ErrorMessage"] = "The file contains no valid records.";
            return RedirectToAction("Bulk_Check");
        }

        return null;
    }

    private async Task<IActionResult> SubmitAndHandleResponseAsync<TBulk>(
        TBulk bulkRequest,
        Func<TBulk, Task<CheckEligibilityResponseBulk>> submitFunc,
        int numberOfRecords,
        string filename)
    {
        try
        {
            var response = await submitFunc(bulkRequest);
            if (response?.Links?.Get_BulkCheck_Status != null)
            {
                HttpContext.Session.SetString("BulkCheckUrl", response.Links.Get_BulkCheck_Status);

                var fileSubmittedViewModel = new BulkCheckFileSubmittedViewModel
                {
                    Filename = filename,
                    NumberOfRecords = numberOfRecords
                };
                return RedirectToAction("Bulk_Check_History");
            }

            TempData["ErrorMessage"] = "Failed to submit bulk check. Please try again.";
            return RedirectToAction("Bulk_Check");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting bulk check");
            TempData["ErrorMessage"] = "An error occurred while submitting the bulk check. Please try again.";
            return RedirectToAction("Bulk_Check");
        }
    }
}
