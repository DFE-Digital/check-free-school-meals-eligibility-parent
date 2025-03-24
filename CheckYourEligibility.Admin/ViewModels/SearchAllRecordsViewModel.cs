using CheckYourEligibility.Domain.Responses;

namespace CheckYourEligibility.Admin.ViewModels
{
    public class SearchAllRecordsViewModel
    {
        public Models.ApplicationSearch? ApplicationSearch { get; set; } = new Models.ApplicationSearch();

        public bool Selected { get; set; }
        public ApplicationResponse? Person { get; set; }
        public string? DetailView { get; set; }
        public bool ShowSelectorCheck { get; internal set; }
        public bool ShowSchool { get; internal set; }
        public bool ShowParentDob { get; internal set; }


        public List<SearchAllRecordsViewModel>? People { get; set; }
        public SearchAllRecordsViewModel()
        {
            People = new List<SearchAllRecordsViewModel>();
        }


        public IEnumerable<string> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected people:
            return (from p in this.People where p.Selected select p.Person.Id).ToList();
        }
    }
}
