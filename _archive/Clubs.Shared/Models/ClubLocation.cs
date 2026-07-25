namespace Clubs.Shared.Models;

/// <summary>
/// A single pinned point of interest (e.g. a rowing course, a cricket ground,
/// a meeting point) that comes from the database. Kept deliberately generic
/// so it works for either club.
/// </summary>
public class ClubLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Shown in the brief popup when the pin is tapped.
    public string ShortDescription { get; set; } = string.Empty;

    // Shown only on the full details page.
    public string FullDescription { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    // Downloadable files attached to this location, e.g. a .gpx route.
    public List<DownloadableFile> Files { get; set; } = new();
}

public class DownloadableFile
{
    public string FileName { get; set; } = string.Empty;   // e.g. "riverside-loop.gpx"
    public string Url { get; set; } = string.Empty;         // where to download it from
    public string? Description { get; set; }                // e.g. "5km loop, easy grade"
}
