using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.ViewModels;

public class SearchAllRecordsViewModel
{
    public SearchAllRecordsViewModel()
    {
        People = new List<SearchAllRecordsViewModel>();
    }

    public ApplicationSearch? ApplicationSearch { get; set; } = new();

    public bool Selected { get; set; }
    public ApplicationResponse? Person { get; set; }
    public string? DetailView { get; set; }
    public bool ShowSelectorCheck { get; internal set; }
    public bool ShowSchool { get; internal set; }
    public bool ShowParentDob { get; internal set; }


    public List<SearchAllRecordsViewModel>? People { get; set; }


    public IEnumerable<string> getSelectedIds()
    {
        // Return an Enumerable containing the Id's of the selected people:
        return (from p in People where p.Selected select p.Person.Id).ToList();
    }
}