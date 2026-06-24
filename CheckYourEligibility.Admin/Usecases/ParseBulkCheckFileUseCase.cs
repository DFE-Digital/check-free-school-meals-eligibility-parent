using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using System.Globalization;
using System.Text;
using static CheckYourEligibility.Admin.Helpers.CsvBulkCheckValidatorHelper;

namespace CheckYourEligibility.Admin.Usecases
{

    public interface IParseBulkCheckFileUseCase
    {
        Task<BulkCheckCsvResult<TRequest>> Execute<TRequest>(Stream csvStream, 
            Func<IReaderRow, int,string?, TRequest> createRequestItem,
            string[] expectedHeaders, 
            int organisationId,
            OrganisationCategory organisationType,
            string? schoolUrn = null) where TRequest : CheckEligibilityRequestDataBase;
    }

    public class ParseBulkCheckFileUseCase : IParseBulkCheckFileUseCase
    {
        private readonly IConfiguration _config;
        private readonly ICheckGateway _checkGateway;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _rowCountLimit;

        public ParseBulkCheckFileUseCase(IServiceProvider serviceProvider, IConfiguration configuration, ICheckGateway checkGateway)
        {
            _serviceProvider = serviceProvider;
            _config = configuration;
            _checkGateway = checkGateway;
            _rowCountLimit = int.Parse(_config["BulkEligibilityCheckLimit"] ?? "5000");
        }

        public async Task<BulkCheckCsvResult<TRequest>> Execute<TRequest>(Stream csvStream,
            Func<IReaderRow, int, string?, TRequest> createRequestItem,
            string[] expectedHeaders, 
            int organisationId,
            OrganisationCategory organisationType,
            string? schoolUrn = null) where TRequest : CheckEligibilityRequestDataBase
        {

            var validator = _serviceProvider.GetService<IValidator<TRequest>>()
                        ?? throw new InvalidOperationException($"No IValidator<{typeof(TRequest).Name}> registered");

            HashSet<int>? schoolUrnHashSet = null;
            //retrieve list of school urn for the org
            if (organisationType == OrganisationCategory.LocalAuthority)
            {
                // call endpoint anad generate hashset
                var schools = await _checkGateway.GetSchoolsAsync(organisationId);
                schoolUrnHashSet =  new HashSet<int>(schools.Data.Select(s => s.URN));

            }
            else if (organisationType == OrganisationCategory.MultiAcademyTrust) {

                // call endpoint and generate hashset
                var academies = await _checkGateway.GetAcademiesAsync(organisationId);
                schoolUrnHashSet = new HashSet<int>(academies.Data.Select(s => s.URN));
            }

          var result = await ParseBulkCsvAsync(csvStream,
              validator, expectedHeaders, createRequestItem, _rowCountLimit, schoolUrn, schoolUrnHashSet, organisationType);
            return result;
        }

        /// <summary>
        /// Validation of csv headers and csv rows for the passed object of type CheckEligibilityRequestDataBase
        /// </summary>
        /// <returns></returns>
        private async Task<BulkCheckCsvResult<TRequest>> ParseBulkCsvAsync<TRequest>(
        Stream csvStream,
        IValidator<TRequest> validator,
        string[] expectedHeaders,
        Func<IReaderRow, int, string?, TRequest> createRequestItem,
        int rowCountLimit, string? schoolUrn, HashSet<int> schoolUrnHashSet, OrganisationCategory organisationType)
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
                        var requestItem = createRequestItem(csv, sequence, schoolUrn);
                        // create context for validator to apply correct validation rules
                        var validationContext = new ValidationContext<TRequest>(requestItem);
                        validationContext.RootContextData["validSchoolUrns"] = schoolUrnHashSet;
                        validationContext.RootContextData["organisationType"] = organisationType;
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


    }
}