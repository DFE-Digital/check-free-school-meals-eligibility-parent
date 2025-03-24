using CsvHelper.Configuration.Attributes;

namespace CheckYourEligibility.Admin.Models
{
    public class ApplicationExport
    {

        [Name("Reference")]
        public string Reference { get; set; }

        [Name("Parent or Guardian")]
        public string Parent { get; set; }

        [Name("Child")]
        public string Child { get; set; }

        [Name("Child Date of Birth")]
        public string ChildDOB { get; set; }

        [Name("Status")]
        public string Status { get; set; }

        [Name("Submission Date")]
        public string SubmisionDate { get; set; }
    }
}
