namespace BoltonWalking.App.Models;

public enum FeedbackCategory
{
    General,
    App,
    RouteWalk
}

/// <summary>
/// Member feedback, written to the "feedback" Firestore collection (public
/// create, admin-only read - same shape as "route-submissions") for the
/// committee to review via docs\admin.html.
/// </summary>
public class FeedbackSubmission
{
    public FeedbackCategory Category { get; set; }

    public string SubmitterName { get; set; } = string.Empty;
    public string SubmitterEmail { get; set; } = string.Empty;

    // Shared across all three categories.
    public int? Rating { get; set; }
    public string? Comments { get; set; }

    // App feedback only.
    public string? AppFeedbackType { get; set; }
    public string? DevicePlatform { get; set; }
    public string? DeviceModel { get; set; }

    // Route/walk feedback only - EventLabel is denormalised (route name +
    // date) purely so the admin page can display it without a join.
    public string? EventId { get; set; }
    public string? EventLabel { get; set; }
    public string? WalkQuality { get; set; }
    public string? WalkOrganisation { get; set; }
    public string? BookingExperience { get; set; }
}
