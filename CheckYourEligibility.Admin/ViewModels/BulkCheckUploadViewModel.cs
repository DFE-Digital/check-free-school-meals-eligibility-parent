namespace CheckYourEligibility.Admin.ViewModels
{
    public class BulkCheckUploadViewModel
    {
        public bool isSchool { get; set; }
        public bool isEnhanced { get; set; }
        public List<string> GuidanceItems { get; set; }
    }
}