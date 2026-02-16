namespace CheckYourEligibility.Admin.ViewModels
{
    public class BulkCheckFsmBasicViewModel
    {
        public string DocumentTemplatePath { get; set; } = string.Empty;
        public List<string> FieldDescriptions { get; set; } = new();
    }

    public class CheckRowErrorFsmBasic
    {
        public int LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BulkCheckFsmBasicErrorsViewModel
    {
        public string Response { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<CheckRowErrorFsmBasic> Errors { get; set; } = Enumerable.Empty<CheckRowErrorFsmBasic>();
        public int TotalErrorCount { get; set; }
    }

    public class BulkCheckFsmBasicStatusViewModel
    {
        public string BulkCheckId { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public int? NumberOfRecords { get; set; }
        public string? FinalNameInCheck { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class BulkCheckFsmBasicStatusesViewModel
    {
        public BulkCheckFsmBasicStatusesViewModel()
        {
            Checks = new List<BulkCheckFsmBasicStatusViewModel>();
        }

        public List<BulkCheckFsmBasicStatusViewModel> Checks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

    public class BulkCheckFsmBasicFileSubmittedViewModel
    {
        public string Filename { get; set; } = string.Empty;
        public int? NumberOfRecords { get; set; }
    }
}
