namespace Clubs.Shared.Models;

public class EventItem
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
