using CheckYourEligibility.Admin.Boundary.Responses;
using static CheckYourEligibility.Admin.ViewModels.ReportHistoryViewModel;

namespace CheckYourEligibility.Admin.ViewModels
{
    public class ReportHistoryViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public IEnumerable<ReportHistoryItemViewModel>? Data { get; set; }
        public class StatusBanner
        {
            public string Status { get; set; }
            public string ColorTag { get; set; }
            public bool CanDownload { get; set; }
            public bool CanDelete { get; set; }

            public StatusBanner(string status)
            {
                switch (status.ToLower())
                {
                    case "new":
                        Status = "Not started";
                        ColorTag = "govuk-tag--grey";
                        CanDownload = false;
                        CanDelete = false;
                        break;

                    case "generating":
                        Status = "In progress";
                        ColorTag = "govuk-tag--blue";
                        CanDownload = false;
                        CanDelete = false;
                        break;

                    case "complete":
                        Status = "Complete";
                        ColorTag = "govuk-tag--green";
                        CanDownload = true;
                        CanDelete = true;
                        break;

                    case "failed":
                        Status = "System error - try again";
                        ColorTag = "govuk-tag--red";
                        CanDownload = false;
                        CanDelete = true;
                        break;
                }
            }
        }

    }

    public class ReportHistoryItemViewModel
    {
        public EligibilityCheckReportHistoryItem Item { get; set; }
        public StatusBanner StatusBanner { get; set; }
    }

}
