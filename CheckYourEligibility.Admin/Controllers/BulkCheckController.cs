using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Validation;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckYourEligibility.Admin.Controllers;

public class BulkCheckController : BaseController
{
    private const int TotalErrorsToDisplay = 20;
    private readonly ICheckGateway _checkGateway;
    private readonly IConfiguration _config;
    private readonly ILogger<BulkCheckController> _logger;
    private ILogger<BulkCheckController> _loggerMock;
    protected DfeClaims? _Claims;

    public BulkCheckController(ILogger<BulkCheckController> logger, ICheckGateway checkGateway,
        IConfiguration configuration)
    {
        _config = configuration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _checkGateway = checkGateway ?? throw new ArgumentNullException(nameof(checkGateway));
    }

    public IActionResult Bulk_Check()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Bulk_Check(IFormFile fileUpload)
    {
        var timeNow = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("FirstSubmissionTimeStamp")))
        {
            var firstSubmissionTimeStampString = HttpContext.Session.GetString("FirstSubmissionTimeStamp");
            DateTime.TryParse(firstSubmissionTimeStampString, out var firstSubmissionTimeStamp);
            var timein1Hour = firstSubmissionTimeStamp.AddHours(1);

            if (timeNow >= timein1Hour) HttpContext.Session.Remove("BulkSubmissions");
        }

        TempData["Response"] = "data_issue";
        List<CheckRow> DataLoad;
        var errorCount = 0;
        var requestItems = new List<CheckEligibilityRequestData_Fsm>();
        var validationResultsItems = new StringBuilder();
        if (fileUpload == null || fileUpload.ContentType.ToLower() != "text/csv")
        {
            TempData["ErrorMessage"] = "Select a CSV File";
            return RedirectToAction("Bulk_Check");
        }

        // limit csv submission attempts
        var sessionCount = 0;
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("BulkSubmissions")))
        {
            // set session value as 0 if it didnt exist
            HttpContext.Session.SetInt32("BulkSubmissions", 0);
            // Set time in its own session value
            HttpContext.Session.SetString("FirstSubmissionTimeStamp", DateTime.UtcNow.ToString());
        }
        else
        {
            // if it exists, get the value
            sessionCount = (int)HttpContext.Session.GetInt32("BulkSubmissions");
        }

        // increment
        sessionCount++;
        HttpContext.Session.SetInt32("BulkSubmissions", sessionCount);

        // validate
        if (sessionCount > int.Parse(_config["BulkUploadAttemptLimit"]))
        {
            TempData["ErrorMessage"] =
                $"No more than {_config["BulkUploadAttemptLimit"]} batch check requests can be made per hour";
            return RedirectToAction("Bulk_Check");
        }

        // check not more than 10, if it is return Bulk_Check() with ErrorMessage == too many requests made, wait a bit longer


        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null
            };
            using (var fileStream = fileUpload.OpenReadStream())

            using (var csv = new CsvReader(new StreamReader(fileStream), config))
            {
                csv.Context.RegisterClassMap<CheckRowRowMap>();
                DataLoad = csv.GetRecords<CheckRow>().ToList();

                // if it has a header record add one to the limit
                var checkRowLimit = int.Parse(_config["BulkEligibilityCheckLimit"]);

                if (DataLoad.Count > checkRowLimit)
                {
                    TempData["ErrorMessage"] = $"CSV File cannot contain more than {checkRowLimit} records";
                    return RedirectToAction("Bulk_Check");
                }

                if (DataLoad == null || !DataLoad.Any()) throw new InvalidDataException("Invalid file content.");

                //Check headers match template
                var expectedHeaders = new[] { "Parent National Insurance number", "Parent asylum support reference number", "Parent Date of Birth", "Parent Last Name" };
                var actualHeaders = csv.HeaderRecord;
                if (!expectedHeaders.SequenceEqual(actualHeaders))
                {
                    TempData["ErrorMessage"] = "The column headings in the selected file must exactly match the template";
                    return RedirectToAction("Bulk_Check");
                }
            }

            var validator = new CheckEligibilityRequestDataValidator_Fsm();
            var sequence = 1;


            foreach (var item in DataLoad)
            {
                var requestItem = new CheckEligibilityRequestData_Fsm
                {
                    LastName = item.LastName,
                    DateOfBirth = DateTime.TryParse(item.DOB, out var dtval)
                        ? dtval.ToString("yyyy-MM-dd")
                        : string.Empty,
                    NationalInsuranceNumber = item.Ni.ToUpper(),
                    NationalAsylumSeekerServiceNumber = item.Nass.ToUpper(),
                    Sequence = sequence
                };
                var validationResults = validator.Validate(requestItem);
                if (!validationResults.IsValid)
                    errorCount = checkIfExists(sequence, validationResultsItems, validationResults, errorCount);
                else
                    requestItems.Add(requestItem);
                sequence++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("ImportEstablishmentData", ex);
            validationResultsItems.AppendLine(ex.Message);
        }

        if (validationResultsItems.Length > 0)
        {
            if (errorCount - TotalErrorsToDisplay > 0)
                TempData["BulkParentCheckItemsLineMoreErrors"] = errorCount - TotalErrorsToDisplay;

            TempData["BulkParentCheckItemsErrors"] = validationResultsItems.ToString();
            return View("BulkOutcome/Error_Data_Issue");
        }

        var result = await _checkGateway.PostBulkCheck(new CheckEligibilityRequestBulk_Fsm { Data = requestItems });
        HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
        HttpContext.Session.SetString("Get_BulkCheck_Results", result.Links.Get_BulkCheck_Results);
        return RedirectToAction("Bulk_Loader");
    }

    public async Task<IActionResult> Bulk_Loader()
    {
        var result = await _checkGateway.GetBulkCheckProgress(HttpContext.Session.GetString("Get_Progress_Check"));
        if (result != null)
        {
            TempData["totalCounter"] = result.Data.Total;
            TempData["currentCounter"] = result.Data.Complete;
            if (result.Data.Complete >= result.Data.Total) return RedirectToAction("Bulk_check_success");
        }

        return View();
    }

    public async Task<IActionResult> Bulk_check_success()
    {
        _Claims = DfeSignInExtensions.GetDfeClaims(HttpContext.User.Claims);
        OrganisationCategory organisationType = _Claims.Organisation.Category.Id;
        TempData["organisationType"] = organisationType;

        return View("BulkOutcome/Success");
    }

    public async Task<IActionResult> Bulk_check_download()
    {
        var resultData =
            await _checkGateway.GetBulkCheckResults(HttpContext.Session.GetString("Get_BulkCheck_Results"));
        var exportData = resultData.Data.Select(x => new BulkFSMExport
        {
            LastName = x.LastName,
            DOB = x.DateOfBirth,
            NI = x.NationalInsuranceNumber,
            NASS = x.NationalAsylumSeekerServiceNumber,
            Outcome = x.Status.GetFsmStatusDescription()
        });

        var fileName = $"free-school-meal-outcomes-{DateTime.Now.ToString("yyyyMMdd")}.csv";

        var result = WriteCsvToMemory(exportData);
        var memoryStream = new MemoryStream(result);
        return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = fileName };
    }

    private byte[] WriteCsvToMemory(IEnumerable<BulkFSMExport> records)
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

    private int checkIfExists(int sequence, StringBuilder validationResultsItems, ValidationResult validationResults,
        int errorCount)
    {
        var message = "";
        if (errorCount >= TotalErrorsToDisplay)
        {
            errorCount++;
            return errorCount;
        }

        foreach (var item in validationResults.Errors)
            switch (item.ErrorMessage)
            {
                case ValidationMessages.LastName:
                case "'LastName' must not be empty.":
                {
                    message = $"<li>Line {sequence}: Issue with Surname</li>";
                    errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                }
                    break;
                case ValidationMessages.DOB
                    :
                case "'Date Of Birth' must not be empty.":
                {
                    message = $"<li>Line {sequence}: Issue with date of birth</li>";
                    errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                }
                    break;
                case ValidationMessages.NI:
                {
                    message = $"<li>Line {sequence}: Issue with National Insurance number</li>";
                    errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                }
                    break;
                case ValidationMessages.NI_and_NASS:
                {
                    message = $"<li>Line {sequence}: Issue {ValidationMessages.NI_and_NASS}</li>";
                    errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                }
                    break;
                case ValidationMessages.NI_or_NASS:
                {
                    message = $"<li>Line {sequence}: Issue {ValidationMessages.NI_or_NASS}</li>";
                    errorCount = AddLineIfNotExist(validationResultsItems, errorCount, message);
                }
                    break;
                default:
                    message = $"<li>Line {sequence}: Issue {item.ErrorMessage}</li>";
                    if (!validationResultsItems.ToString().Contains(message))
                    {
                        validationResultsItems.AppendLine(message);
                        errorCount++;
                    }

                    break;
            }

        return errorCount;
    }

    private static int AddLineIfNotExist(StringBuilder validationResultsItems, int errorCount, string message)
    {
        if (!validationResultsItems.ToString().Contains(message))
        {
            validationResultsItems.AppendLine(message);
            errorCount++;
        }

        return errorCount;
    }
}