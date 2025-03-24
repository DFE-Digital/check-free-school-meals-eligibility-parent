using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;

namespace CheckYourEligibility.Admin.Models
{
    public class ApplicationSearch
    {
        // Pagination Properties
        public int PageNumber { get; set; } = 1; // Default to page 1
        public int PageSize { get; set; } = 10; // Default to 10 items per page

        public int? LocalAuthority { get; set; }
        public int? School { get; set; }
        public List<ApplicationStatus> Status { get; set; } = new List<ApplicationStatus>();

        [LastName("last name", "Child", "ChildIndex")]
        public string? ChildLastName { get; set; }

        [LastName("last name", "Parent or guardian", "ChildIndex")]
        public string? ParentLastName { get; set; }

        [ReferenceNumber]
        public string? Reference { get; set; }

        [NotMapped]
        [Dob("date of birth", "child", "ChildIndex", "ChildDobDay", "ChildDobMonth", "ChildDobYear", isRequired: false, applyAgeRange: true)]
        public string? ChildDob { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day for child using numbers only ")]
        public string? ChildDobDay { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a month for child using numbers only")]
        public string? ChildDobMonth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a year for child using numbers only")]
        public string? ChildDobYear { get; set; }

        [NotMapped]
        [Dob("date of birth", "parent or guardian", null, "PGDobDay", "PGDobMonth", "PGDobYear", isRequired: false, applyAgeRange: false)]
        public string? ParentDob { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day for parent or guardian using numbers only")]
        public string? PGDobDay { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a month for parent or guardian using numbers only")]
        public string? PGDobMonth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a year for parent or guardian using numbers only")]
        public string? PGDobYear { get; set; }
        public string? Keyword { get; set; }
        public DateRange? DateRange { get; set; }
    }
    public class DateRange
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
    

