namespace CheckYourEligibility.Admin.ViewModels
{
    public class CheckRowError_arc
    {
        public int LineNumber { get; set; }
        public string Message { get; set; }
    }

    public class BulkCheckErrorsViewModel_arch
    {
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<CheckRowError> Errors { get; set; }
        public int TotalErrorCount {  get; set; }
    }
}