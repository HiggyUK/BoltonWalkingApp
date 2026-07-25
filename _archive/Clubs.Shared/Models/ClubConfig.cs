namespace Clubs.Shared.Models;

/// <summary>
/// Everything that makes "the app for Club A" look and feel different from
/// "the app for Club B", without duplicating a single line of app logic.
/// Each App head (ClubA.App / ClubB.App) constructs one of these in its
/// MauiProgram.cs and registers it with DI - that's the only branching point.
/// </summary>
public class ClubConfig
{
    public string ClubName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    // Hex strings so they're easy to store in JSON/config later if you want
    // to move this out of code and into a CMS or remote config service.
    public string PrimaryColorHex { get; set; } = "#512BD4";
    public string SecondaryColorHex { get; set; } = "#2B0B98";

    public string LogoImageResource { get; set; } = "logo.png";
    public string ContactEmail { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;

    // Example of a per-club feature flag - handy once the clubs diverge
    // slightly in what they need (e.g. one club takes online payments, one doesn't).
    public bool MembershipPaymentsEnabled { get; set; } = false;
}
