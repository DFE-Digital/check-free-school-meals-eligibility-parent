using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        public ParentGuardian parentDetails { get; set; }

        public Child[] children { get; set; }
    }
}
