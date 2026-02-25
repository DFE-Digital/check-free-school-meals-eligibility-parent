using System.ComponentModel.DataAnnotations.Schema;
using CheckYourEligibility.Admin.Attributes;

namespace CheckYourEligibility.Admin.Models;

public class MenuItem
{
    public string MenuText { get; set; }
    public string TileText { get; set; }
    public string TileSubText { get; set; }
    public string UrlController { get; set; }
    public string UrlView { get; set; }

    public MenuItem(string menuText, string tileText, string tileSubText, string urlController, string urlView)
    {
        MenuText = menuText;
        TileText = tileText;
        TileSubText = tileSubText;
        UrlController = urlController;
        UrlView = urlView;
    }
}