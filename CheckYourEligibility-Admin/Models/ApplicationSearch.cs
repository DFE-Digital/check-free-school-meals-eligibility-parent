﻿using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class ApplicationSearch
    {
        // Pagination Properties
        public int PageNumber { get; set; } = 1; // Default to page 1
        public int PageSize { get; set; } = 10; // Default to 10 items per page
        //
        public int? LocalAuthority { get; set; }
        public int? School { get; set; }
        public ApplicationStatus? Status { get; set; }

        [LastName(ErrorMessage = "Child last name field contains an invalid character")]
        public string? ChildLastName { get; set; }

        [LastName(ErrorMessage = "Parent or Guardian last name field contains an invalid character")]
        public string? ParentLastName { get; set; }

        [ReferenceNumber]
        public string? Reference { get; set; }

        [NotMapped]
        [Dob("ChildDobDay", "ChildDobMonth", "ChildDobYear", isRequired: false, applyAgeRange: true)]
        public string? ChildDob { get; set; }

        public string? ChildDobDay { get; set; }

        public string? ChildDobMonth { get; set; }

        public string? ChildDobYear { get; set; }

        [NotMapped]
        [Dob("PGDobDay", "PGDobMonth", "PGDobYear", isRequired: false, applyAgeRange: false)]
        public string? ParentDob { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a day using numbers only")]
        public string? PGDobDay { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a month using numbers only")]
        public string? PGDobMonth { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Enter a year using numbers only")]
        public string? PGDobYear { get; set; }
        

    }
}
