namespace CheckYourEligibility.Admin.ViewModels
{
    public class ApplicationConfirmationEntitledViewModel
    {
        public string ParentName { get; set; }

        public List<ApplicationConfirmationEntitledChildViewModel> Children { get; set; }
    }
    public class ApplicationConfirmationEntitledChildViewModel
    {
        public string ParentName { get; set; }

        public string ChildName { get; set; }


        public string Reference { get; set; }
    }
}
