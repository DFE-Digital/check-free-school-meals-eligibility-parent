using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Models
{
    public class School
    {
        [Required(ErrorMessage = "School is required")]
        public string Name { get; set; }

        public string? URN { get; set; }

        public string? LA { get; set; }

        public string? Postcode { get; set; }
    }
}
