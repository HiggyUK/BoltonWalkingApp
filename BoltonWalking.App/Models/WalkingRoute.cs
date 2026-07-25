namespace BoltonWalking.App.Models;

public enum RouteDifficulty
{
    Easy,
    Moderate,
    Hard
}

/// <summary>
/// A single pinned walking route shown on the Routes map. Fields are split
/// into sections (rather than one description blob) to match how
/// boltonwalking.co.uk/routes-and-venues itself presents each route.
/// </summary>
public class WalkingRoute
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public RouteDifficulty Difficulty { get; set; }

    // Exact badge text from the site, e.g. "🟠🟠 (in daylight) / 🔴🔴 (in darkness)".
    public string DifficultyBadge { get; set; } = string.Empty;

    // Shown in the brief popup when a pin is tapped, e.g. "7km - 70m ascent - 2 hours".
    public string ShortDescription { get; set; } = string.Empty;

    // ⛰️ distance / ascent / duration line.
    public string Stats { get; set; } = string.Empty;

    // The bold terrain/suitability paragraph.
    public string TerrainNotes { get; set; } = string.Empty;

    // 📍 starting venue.
    public string Venue { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string What3Words { get; set; } = string.Empty;
    public string GridReference { get; set; } = string.Empty;

    // 🚌/🚍 public transport notes.
    public string TransportNotes { get; set; } = string.Empty;

    // 🚗 parking notes.
    public string ParkingNotes { get; set; } = string.Empty;

    // Real photos from the route's gallery on the club website, shown as a
    // rotating carousel. Empty list is fine - the carousel just won't show.
    public List<string> PhotoUrls { get; set; } = new();

    // Downloadable files attached to this route (GPX track, risk assessment PDF).
    public List<DownloadableFile> Files { get; set; } = new();
}

public class DownloadableFile
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
}
