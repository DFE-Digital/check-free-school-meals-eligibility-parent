using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Validation;
using CheckYourEligibility.Admin.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckYourEligibility.Admin.Usecases
{
    public class BulkCheckCsvResultFsmBasic
    {
        public List<CheckEligibilityRequestData_FsmBasic> ValidRequests { get; set; } = new();
        public List<CsvRowErrorFsmBasic> Errors { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class CsvRowErrorFsmBasic
    {
        public int LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public interface IParseBulkCheckFileUseCase_FsmBasic
    {
        Task<BulkCheckCsvResultFsmBasic> Execute(Stream stream);
    }

    public class ParseBulkCheckFileUseCase_FsmBasic : IParseBulkCheckFileUseCase_FsmBasic
    {
        private readonly IValidator<CheckEligibilityRequestData_FsmBasic> _validator;
        private readonly IConfiguration _config;
        private readonly int _rowCountLimit;

        public ParseBulkCheckFileUseCase_FsmBasic(IValidator<CheckEligibilityRequestData_FsmBasic> validator, IConfiguration configuration)
        {
            _validator = validator;
            _config = configuration;
            _rowCountLimit = int.Parse(_config["BulkEligibilityCheckLimit"] ?? "1000");
        }

        public async Task<BulkCheckCsvResultFsmBasic> Execute(Stream csvStream)
        {
            string[] expectedHeaders = { "parent first name", "parent last name", "parent date of birth", "parent national insurance number", "parent asylum seeker reference number" };

            var result = new BulkCheckCsvResultFsmBasic();

            using var reader = new StreamReader(csvStream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);

            var lineNumber = 2; // headers on line 1
            var sequence = 1;

            try
            {
                // Read and validate headers
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                if (headers == null || headers.Length < expectedHeaders.Length)
                {
                    result.ErrorMessage = "The column headers in the selected file must exactly match the template";
                    return result;
                }

                // Normalize headers for comparison
                var normalizedHeaders = headers.Select(h => h.Trim().ToLowerInvariant()).ToArray();

                // Check if all expected headers are present
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!normalizedHeaders.Contains(expectedHeader))
                    {
                        result.ErrorMessage = $"Invalid CSV format. Missing required header: '{expectedHeader}'";
                        return result;
                    }
                }

                // Read data rows
                while (await csv.ReadAsync())
                {
                    if (sequence > _rowCountLimit)
                    {
                        result.ErrorMessage = $"CSV file cannot contain more than {_rowCountLimit} records";
                        break;
                    }

                    try
                    {
                        var lastName = csv.GetField("Parent Last Name")?.Trim() ?? string.Empty;
                        var dob = csv.GetField("Parent Date of Birth")?.Trim() ?? string.Empty;
                        var ni = csv.GetField("Parent National Insurance Number")?.Trim() ?? string.Empty;

                        // Parse date if needed
                        var dobFormatted = dob;
                        if (DateTime.TryParse(dob, out var dtval))
                        {
                            dobFormatted = dtval.ToString("yyyy-MM-dd");
                        }

                        var requestItem = new CheckEligibilityRequestData_FsmBasic
                        {
                            LastName = lastName,
                            DateOfBirth = dobFormatted,
                            NationalInsuranceNumber = ni.ToUpper(),
                            Sequence = sequence
                        };

                        var validationResults = await _validator.ValidateAsync(requestItem);

                        if (!validationResults.IsValid)
                        {
                            foreach (var error in validationResults.Errors)
                            {
                                if (!ContainsError(result.Errors, lineNumber, error.ErrorMessage))
                                {
                                    result.Errors.Add(new CsvRowErrorFsmBasic
                                    {
                                        LineNumber = lineNumber,
                                        Message = error.ErrorMessage
                                    });
                                }
                            }
                        }
                        else
                        {
                            result.ValidRequests.Add(requestItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new CsvRowErrorFsmBasic
                        {
                            LineNumber = lineNumber,
                            Message = $"Error parsing row: {ex.Message}"
                        });
                    }

                    lineNumber++;
                    sequence++;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error reading CSV file: {ex.Message}";
            }

            return result;
        }

        private bool ContainsError(IEnumerable<CsvRowErrorFsmBasic> errors, int lineNumber, string errorMessage)
        {
            return errors.Any(e => e.LineNumber == lineNumber && e.Message == errorMessage);
        }
    }
}
