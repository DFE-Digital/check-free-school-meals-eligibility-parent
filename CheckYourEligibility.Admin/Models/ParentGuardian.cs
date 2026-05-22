using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Models
{
    public sealed class ParentGuardian : ParentGuardianBasic
    {

        [EmailAddress]
        [Required(ErrorMessage = "Enter an email")]
        public string? EmailAddress { get; set; }

        [Nass(nameof(NinAsrSelection))][MaxLength(10)] public string? NationalAsylumSeekerServiceNumber { get; set; }

    }
}