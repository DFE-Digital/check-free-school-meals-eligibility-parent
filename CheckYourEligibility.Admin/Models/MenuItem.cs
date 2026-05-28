namespace CheckYourEligibility.Admin.Models;

public class MenuItem
{
    public string MenuText { get; set; }
    public string TileText { get; set; }
    public string TileSubText { get; set; }
    public string UrlController { get; set; }
    public string UrlView { get; set; }
    public string? FeatureName { get; set; }
    public bool ShowInHeader { get; set; } = true;
    public bool ShowAsTile { get; set; } = true;


    public MenuItem(string menuText, string tileText, string tileSubText, string urlController, string urlView, string? featureName = null, bool showInHeader = true, bool showAsTile= true )
    {
        MenuText = menuText;
        TileText = tileText;
        TileSubText = tileSubText;
        UrlController = urlController;
        UrlView = urlView;
        FeatureName = featureName;
        ShowInHeader = showInHeader;
        ShowAsTile = showAsTile;
    }
}