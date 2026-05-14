using CheckYourEligibility.Admin.Domain.Enums;
public class EligibilityCheckReportViewModel 
{
    public string DateRange { get; set; }

    public DateTime StartDateValue
    {
        get
        {
            return DateRange switch
            {
                "30-days" => DateTime.UtcNow.AddDays(-30)            
            };
        }
    }
    public DateTime EndDateValue
    {
        get
        {
            return DateTime.UtcNow;
        }
    }
    public CheckType CheckType { get; set; }
}
