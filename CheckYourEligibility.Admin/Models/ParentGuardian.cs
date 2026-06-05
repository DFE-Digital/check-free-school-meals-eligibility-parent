using CheckYourEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility.Admin.Models
{
    public sealed class ParentGuardian
    {
        [Name]
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }

        [Name]
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? EmailAddress { get; set; }

        [NotMapped]
        [Dob("date of birth", "parent or guardian", null, "Day", "Month", "Year")]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

        [Required(ErrorMessage = "Enter a National Insurance number")]
        [MaxLength(13)] public string? NationalInsuranceNumber { get; set; }
    }
}
