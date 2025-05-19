using System.Collections.ObjectModel;

namespace CheckYourEligibility.FrontEnd.UseCases
{
    public class EvidenceFileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IValidateEvidenceFileUseCase
    {
        EvidenceFileValidationResult Execute(IFormFile file);
    }

    public class ValidateEvidenceFileUseCase : IValidateEvidenceFileUseCase
    {
        private readonly ILogger<ValidateEvidenceFileUseCase> _logger;
        private readonly ReadOnlyCollection<string> _validTypes = new ReadOnlyCollection<string>(
            new string[] { "image/bmp", "image/jpeg", "image/heic", "image/png", "image/tiff", "application/pdf" });

        public ValidateEvidenceFileUseCase(ILogger<ValidateEvidenceFileUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public EvidenceFileValidationResult Execute(IFormFile file)
        {

            var fileContentType = file.ContentType;
            var fileLength = file.Length;
            var error = string.Empty;
            var valid = true;


            if (!_validTypes.Contains(fileContentType))
            {
                valid = false;
                error = "The selected file must be a JPG, JPEG, HEIC, HEIF, BMP, PNG, TIF, or PDF";
            }

            if (fileLength > 10000000)
            {
                valid = false;
                error = "The selected file must be smaller than 10MB";
            }

            var result = new EvidenceFileValidationResult
            {
                IsValid = valid,
                ErrorMessage = error
            };

            return result;
        }
    }
}
