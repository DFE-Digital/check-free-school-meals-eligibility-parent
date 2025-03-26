using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.ViewModels;

public class SelectPersonEditorViewModel
{
    public bool Selected { get; set; }
    public ApplicationResponse Person { get; set; }
    public string DetailView { get; set; }
    public bool ShowSelectorCheck { get; internal set; }
    public bool ShowSchool { get; internal set; }
    public bool ShowParentDob { get; internal set; }
}

public class PeopleSelectionViewModel
{
    public PeopleSelectionViewModel()
    {
        People = new List<SelectPersonEditorViewModel>();
    }

    public List<SelectPersonEditorViewModel> People { get; set; }


    public IEnumerable<string> getSelectedIds()
    {
        // Return an Enumerable containing the Id's of the selected people:
        return (from p in People where p.Selected select p.Person.Id).ToList();
    }
}