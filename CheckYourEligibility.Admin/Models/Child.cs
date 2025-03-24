using CheckYourEligibility.Admin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility.Admin.Models
{
    public class Child
    {
        [NotMapped]
        public int ChildIndex { get; set; }

        [ChildName("first name")]
        public string? FirstName { get; set; }

        [ChildName("last name")]
        [LastName("last name", "child", "ChildIndex")]
        public string? LastName { get; set; }

        [NotMapped]
        [Dob("date of birth", "child", "ChildIndex", "Day", "Month", "Year", isRequired: true, applyAgeRange: true)]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

        public School? School { get; set; }
    }
}
