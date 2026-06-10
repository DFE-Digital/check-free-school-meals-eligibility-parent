using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.Admin.ViewModels
{
    public class BulkCheckUploadViewModel
    {
        public bool isSchool { get; set; }
        public bool isEnhanced { get; set; }
        public IFormFile? FileUpload { get; set; }
        public string DownloadTemplateController { get; set; } = string.Empty;
        public string DownloadTemplateAction { get; set; } = string.Empty;
        public string FormController { get; set; } = string.Empty;
        public string FormAction { get; set; } = string.Empty;
        public string SubmitButtonText { get; set; } = string.Empty;
        public string HistoryController { get; set; } = string.Empty;
        public string HistoryAction { get; set; } = string.Empty;
        public List<string> GuidanceItems { get; set; }
    }
}