namespace BoltonWalking.App.Models;

/// <summary>
/// A downloadable policy/document PDF, listed under More > Policies &amp;
/// Documents. Read from the "documents" Firestore collection - uploaded
/// and maintained via the admin page's Documents tab.
/// </summary>
public class ClubDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    // Null if the admin page never set one (shouldn't happen - it's required there).
    public DateTime? LastUpdated { get; set; }

    public string LastUpdatedDisplay => LastUpdated is { } date ? $"Last updated {date:d MMMM yyyy}" : string.Empty;
}
