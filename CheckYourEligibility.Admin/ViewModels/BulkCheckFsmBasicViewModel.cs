namespace CheckYourEligibility.Admin.ViewModels
{
    //public class BulkCheckFsmBasicViewModel
    //{
    //    public string DocumentTemplatePath { get; set; } = string.Empty;
    //    public List<string> FieldDescriptions { get; set; } = new();
    //}

    public class CheckRowError
    {
        public int LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BulkCheckErrorsViewModel
    {
        public string Response { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<CheckRowError> Errors { get; set; } = Enumerable.Empty<CheckRowError>();
        public int TotalErrorCount { get; set; }
    }

    public class BulkCheckStatusViewModel
    {
        public string BulkCheckId { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public int? NumberOfRecords { get; set; }
        public string? FinalNameInCheck { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class BulkCheckViewModel
    {
        public BulkCheckViewModel()
        {
            Checks = new List<BulkCheckStatusViewModel>();
        }

        public List<BulkCheckStatusViewModel> Checks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

    public class BulkCheckFileSubmittedViewModel
    {
        public string Filename { get; set; } = string.Empty;
        public int? NumberOfRecords { get; set; }
    }
}
