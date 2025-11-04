using CheckYourEligibility.Admin.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CheckYourEligibility.Admin.Models;

public class UserOrganisation
{
    public OrganisationType Type { get; set; }

    public bool IsLA => Type == OrganisationType.LA;
    public bool IsMAT => Type == OrganisationType.MAT;
    public bool IsSchool => Type == OrganisationType.School;
}
