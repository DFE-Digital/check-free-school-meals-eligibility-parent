using CheckYourEligibility.Admin.Boundary.Requests;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using System.Globalization;
using System.Text;
using static CheckYourEligibility.Admin.Domain.Constants.BulkCheck.BulkCheckUploadConstants;

namespace CheckYourEligibility.Admin.Helpers
{
    public static class CsvBulkCheckValidatorHelper
    {
        public class BulkCheckCsvResult<T>
    where T : CheckEligibilityRequestDataBase
        {
            public List<T> ValidRequests { get; set; } = new();
            public List<CsvRowError> Errors { get; set; } = new();
            public string ErrorMessage { get; set; } = string.Empty;
        }

        public class CsvRowError
        {
            public int LineNumber { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class CsvHeaderValidationResponse
        {

            public bool isSuccess { get; set; }
            public string error { get; set; }
        }
        public const string IncorrectHeadersErrorMessage = "The column headers in the selected file must exactly match the template";
        public const string MissingHeadersErrorMessage = "Invalid CSV format. Missing required header:";


        public static async Task<BulkCheckCsvResult<TRequest>> ParseBulkCsvAsync<TRequest>(
        Stream csvStream,
        IValidator<TRequest> validator,
        string[] expectedHeaders,
        Func<IReaderRow, int, TRequest> createRequestItem,
        int rowCountLimit, bool isEnhancedSchool)
    where TRequest : CheckEligibilityRequestDataBase
        {
            var result = new BulkCheckCsvResult<TRequest>();
            var lineNumber = 2;
            var sequence = 1;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var csvContent = await ReadCsvContent(csvStream);

            using var reader = new StringReader(csvContent);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);
            var headerValidationResponse = await ValidateHeadersAsync(csv, expectedHeaders);

            if (!headerValidationResponse.isSuccess)
            {
                result.ErrorMessage = headerValidationResponse.error;
                return result;
            }

            try
            {
                while (await csv.ReadAsync())
                {
                    if (sequence > rowCountLimit)
                    {
                        result.ErrorMessage = $"CSV file cannot contain more than {rowCountLimit} records";
                        break;
                    }

                    try
                    {
                        var requestItem = createRequestItem(csv, sequence);
                        var validationContext = new ValidationContext<TRequest>(requestItem);
                        validationContext.RootContextData["isEnhancedSchool"] = isEnhancedSchool;
                        var validationResults = await validator.ValidateAsync(validationContext);

                        if (!validationResults.IsValid)
                        {
                            foreach (var error in validationResults.Errors)
                            {
                                if (!result.Errors.Any(e => e.LineNumber == lineNumber && e.Message == error.ErrorMessage))
                                {
                                    result.Errors.Add(new CsvRowError
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
                        result.Errors.Add(new CsvRowError
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

        private static async Task<CsvHeaderValidationResponse> ValidateHeadersAsync(CsvReader csv, string[] expectedHeaders)
        {
            var validationResponse = new CsvHeaderValidationResponse() { isSuccess = true };

            try
            {
                // Read and validate headers
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                if (headers == null || headers.Length < expectedHeaders.Length)
                {
                    validationResponse.isSuccess = false;
                    validationResponse.error = IncorrectHeadersErrorMessage;
                    return validationResponse;
                }

                // Normalize headers for comparison
                var normalizedHeaders = headers.Select(h => h.Trim().ToLowerInvariant()).ToArray();

                // Check if all expected headers are present
                foreach (var expectedHeader in expectedHeaders)
                {
                    if (!normalizedHeaders.Contains(expectedHeader.ToLowerInvariant()))
                    {
                        validationResponse.isSuccess = false;
                        validationResponse.error = MissingHeadersErrorMessage + $" '{expectedHeader}'";
                        return validationResponse;
                    }

                }
            }
            catch (Exception ex)
            {

                validationResponse.isSuccess = false;
                validationResponse.error = $"Error reading CSV file headers: {ex.Message}";
                return validationResponse;
            }
            return validationResponse;
        }

        public static async Task<string> ReadCsvContent(Stream csvStream)
        {
            csvStream.Position = 0;

            using var utf8Reader = new StreamReader(
                csvStream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);

            var content = await utf8Reader.ReadToEndAsync();

            if (!content.Contains('\uFFFD'))
            {
                return content;
            }

            csvStream.Position = 0;

            using var windows1252Reader = new StreamReader(
                csvStream,
                Encoding.GetEncoding(1252),
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);

            return await windows1252Reader.ReadToEndAsync();
        }
        public static string ParseDate(string dob)
        {
            if (DateTime.TryParse(dob, out var dtval))
            {
                return dtval.ToString("yyyy-MM-dd");
            }
            return dob;
        }

        public static CheckEligibilityRequestDataBase CreateRequestItem(IReaderRow csv, int sequence)
        {
            var dob = csv.GetField(ParentDateOfBirthHeader)?.Trim() ?? string.Empty;
            return new CheckEligibilityRequestDataBase
            {
                ParentLastName = csv.GetField(ParentLastNameHeader)?.Trim() ?? string.Empty,
                DateOfBirth = ParseDate(dob),
                NationalInsuranceNumber = csv.GetField(ParentNINOHeader)?.Trim().ToUpper(),
                Sequence = sequence
            };
        }

        public static CheckEligibilityRequestData_Enhanced CreateEnhancedRequestItem(IReaderRow csv, int sequence)
        {
            return new CheckEligibilityRequestData_Enhanced
            {
                ParentFirstName = csv.GetField(ParentFirstNameHeader)?.Trim() ?? string.Empty,
                ParentLastName = csv.GetField(ParentLastNameHeader)?.Trim() ?? string.Empty,
                DateOfBirth = ParseDate(csv.GetField(ParentDateOfBirthHeader)),
                NationalInsuranceNumber = csv.GetField(ParentNINOHeader)?.Trim().ToUpper(),
                ChildFirstName = csv.GetField(ChildFirstNameHeader)?.Trim() ?? string.Empty,
                ChildLastName = csv.GetField(ChildLastNameHeader)?.Trim() ?? string.Empty,
                ChildDateOfBirth = ParseDate(csv.GetField(ChildDateOfBirthHeader)),
                ChildSchoolUrn = csv.GetField(ChildSchoolUrnHeader)?.Trim() ?? string.Empty,
                Sequence = sequence
            };
        }

        public static CheckEligibilityRequestData_Enhanced CreateEnhancedSchoolRequestItem(IReaderRow csv, int sequence)
        {
            return new CheckEligibilityRequestData_Enhanced
            {
                ParentFirstName = csv.GetField(ParentFirstNameHeader)?.Trim() ?? string.Empty,
                ParentLastName = csv.GetField(ParentLastNameHeader)?.Trim() ?? string.Empty,
                DateOfBirth = ParseDate(csv.GetField(ParentDateOfBirthHeader)),
                NationalInsuranceNumber = csv.GetField(ParentNINOHeader)?.Trim().ToUpper(),
                ChildFirstName = csv.GetField(ChildFirstNameHeader)?.Trim() ?? string.Empty,
                ChildLastName = csv.GetField(ChildLastNameHeader)?.Trim() ?? string.Empty,
                ChildDateOfBirth = ParseDate(csv.GetField(ChildDateOfBirthHeader)),
                Sequence = sequence
            };
        }
    }
}



