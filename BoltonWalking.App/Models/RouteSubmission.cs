namespace BoltonWalking.App.Models;

/// <summary>
/// A member-proposed route, written to the "route-submissions" Firestore
/// collection (public create, admin-only read - see docs\admin.html and the
/// Firestore rules note in the route-submission plan) for an admin to
/// review and, if accepted, manually copy into the real "routes" collection.
/// </summary>
public class RouteSubmission
{
    public string? SubmitterName { get; set; }
    public string? SubmitterEmail { get; set; }

    public string RouteName { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public RouteDifficulty Difficulty { get; set; }
    public string? ShortDescription { get; set; }
    public string? TerrainNotes { get; set; }

    // Exactly one of these two is expected - the form enforces that a GPX
    // pick or a route URL is required, not both.
    public string? GpxFileName { get; set; }
    public string? GpxContentBase64 { get; set; }
    public string? RouteUrl { get; set; }

    // Set by the view model before submitting - RouteSubmissionService
    // uploads these to Storage (under "submission-photos/") as part of
    // SubmitAsync and writes their resulting public URLs to the document,
    // rather than the raw bytes here.
    public List<PickedPhoto> PhotosToUpload { get; set; } = new();
}

/// <summary>A picked-but-not-yet-uploaded photo, read into memory by the view model.</summary>
public class PickedPhoto
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "image/jpeg";
}
