using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.ViewModels
{
    public class ApplicationDetailViewModel
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string ParentName { get; set; }
        public string ParentDob { get; set; }
        public string ParentNI { get; set; }
        public string ParentNas { get; set; }
        public string ParentEmail { get; set; }
        public string ChildName { get; set; }
        public string ChildDob { get; set; }
        public string School { get; set; }

    }
}
