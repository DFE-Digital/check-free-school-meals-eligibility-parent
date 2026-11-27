namespace CheckYourEligibility.FrontEnd.Models;

/// <summary>
///     Controls the service notice banner and "Start now" button on the home page.
///     Configured via the "ServiceNotice" section (app settings: ServiceNotice__Enabled, etc.).
/// </summary>
public class ServiceNoticeOptions
{
    public const string SectionName = "ServiceNotice";

    public bool Enabled { get; set; }

    public string? Title { get; set; }

    public string? Heading { get; set; }

    public string? Body { get; set; }

    public bool DisableStartButton { get; set; }

    public bool ShowBanner => Enabled && !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Heading);
}
