﻿using CheckYourEligibility_FrontEnd.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckYourEligibility_FrontEnd.Models
{
    public class Parent
    {   
        [Name]
        [Required(ErrorMessage = "Enter a first name")]
        public string? FirstName { get; set; }
        
        [Name]
        [Required(ErrorMessage = "Enter a last name")]
        public string? LastName { get; set; }

        [NotMapped]
        [Dob("date of birth", "parent", null, "Day", "Month", "Year", isRequired: true, applyAgeRange: false)]
        public string? DateOfBirth { get; set; }

        public string? Day { get; set; }

        public string? Month { get; set; }

        public string? Year { get; set; }

        [Nino]
        [MaxLength(13)]
        public string? NationalInsuranceNumber { get; set; }

        [Nass]
        [MaxLength(10)]
        public string? NationalAsylumSeekerServiceNumber { get; set; }

        public bool? IsNassSelected { get; set; }

        [IsNinoSelected]
        public bool? IsNinoSelected { get; set; }

        public bool NASSRedirect { get; set; }

    }
}
