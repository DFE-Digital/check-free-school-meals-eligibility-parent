using CheckYourEligibility.Admin.Boundary.Requests;
using CsvHelper;
using FluentValidation;
using static CheckYourEligibility.Admin.Helpers.CsvBulkCheckValidatorHelper;

namespace CheckYourEligibility.Admin.Usecases
{

    public interface IParseBulkCheckFileUseCase
    {
        Task<BulkCheckCsvResult<TRequest>> Execute<TRequest>(Stream csvStream, Func<IReaderRow, int,string?, TRequest> createRequestItem, string[] expectedHeaders, bool isEhancedSchool, string? schoolUrn = null) where TRequest : CheckEligibilityRequestDataBase;
    }

    public class ParseBulkCheckFileUseCase : IParseBulkCheckFileUseCase
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _rowCountLimit;

        public ParseBulkCheckFileUseCase(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _config = configuration;
            _rowCountLimit = int.Parse(_config["BulkEligibilityCheckLimit"] ?? "5000");
        }

        public async Task<BulkCheckCsvResult<TRequest>> Execute<TRequest>(Stream csvStream, Func<IReaderRow, int, string?, TRequest> createRequestItem, string[] expectedHeaders, bool isEnhancedSchool, string? schoolUrn = null) where TRequest : CheckEligibilityRequestDataBase
        {

            var validator = _serviceProvider.GetService<IValidator<TRequest>>()
                        ?? throw new InvalidOperationException($"No IValidator<{typeof(TRequest).Name}> registered");

            var result = await ParseBulkCsvAsync(csvStream, validator,expectedHeaders, createRequestItem,  _rowCountLimit, isEnhancedSchool, schoolUrn);
            return result;
        }

    }
}