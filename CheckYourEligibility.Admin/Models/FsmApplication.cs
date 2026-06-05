using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Models;

public class FsmApplication
{
    public string ParentFirstName { get; set; }
    public string ParentLastName { get; set; }
    public string ParentDateOfBirth { get; set; }
    public string ParentNass { get; set; }
    public string ParentNino { get; set; }
    public string ParentEmail { get; set; }
    public Children Children { get; set; }
    [JsonIgnore]
    public List<IFormFile> EvidenceFiles { get; set; }
    public Evidence Evidence { get; set; } = new Evidence { EvidenceList = new List<EvidenceFile>() };
    public string? Tier { get; set; }
    public string? EligibilityEndDate { get; set; }

    [JsonIgnore]
    public FsmStatusBanner? StatusBanner
    {
        get
        {
            if (string.IsNullOrEmpty(Tier))
            {
                return null;
            }
            return new FsmStatusBanner(Tier);
        }
    }

    public class FsmStatusBanner
    {
        public string Status { get; set; }
        public string ColorTag { get; set; }

        public FsmStatusBanner(string? tier)
        {
            switch (tier?.ToLowerInvariant())
            {
                case "expanded":
                    Status = "Eligible expanded";
                    ColorTag = "govuk-tag--green";
                    break;

                case "targeted":
                    Status = "Eligible targeted";
                    ColorTag = "govuk-tag--purple";
                    break;

                default:
                    Status = "Eligible";
                    ColorTag = "govuk-tag--blue";
                    break;
            }
        }
    }

    [JsonIgnore]
    public string? FormattedEligibilityEndDate
    {
        get
        {
            if (string.IsNullOrEmpty(EligibilityEndDate))
            {
                return null;
            }

            if (DateTime.TryParse(EligibilityEndDate, out var date))
            {
                return date.ToString("dd MMMM yyyy"); 
            }

            return EligibilityEndDate; 
        }
    }


}