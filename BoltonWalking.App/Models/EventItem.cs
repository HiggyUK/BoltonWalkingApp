namespace BoltonWalking.App.Models;

/// <summary>
/// A scheduled event from the "events" Firestore collection. Usually a walk -
/// linking a WalkingRoute to a date/time and a Ticket Tailor booking link,
/// joined to its route client-side (see EventsService). Can also be a custom
/// event (e.g. the Christmas party) with no route, whose title, venue and
/// details are entered by hand instead.
/// </summary>
public class EventItem
{
    public const string CustomType = "custom";

    public string Id { get; set; } = string.Empty;

    // "walk" (or missing, for events saved before custom events existed) or "custom".
    public string Type { get; set; } = "walk";
    public bool IsCustom => Type == CustomType;

    public int RouteId { get; set; }

    // Custom events only - walks take these from their Route instead.
    public string Title { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string TicketLink { get; set; } = string.Empty;

    // When booking opens for this walk. Not explicitly set in Firestore means
    // "apply the club's standard rule" (see ComputeStandardBookingOpensAt) -
    // EventsService resolves that fallback, so by the time it reaches the UI
    // this always reflects the real opening time, never a placeholder.
    public DateTime BookingOpensAt { get; set; }

    // Populated by EventsService after fetching both collections - null if
    // the route was deleted/renumbered after the event was created.
    public WalkingRoute? Route { get; set; }

    // Card/detail-page text that works for both walks and custom events.
    public string DisplayTitle => IsCustom ? Title : Route?.Name ?? "Route no longer available";
    public string DisplayVenue => IsCustom ? Venue : Route?.Venue ?? string.Empty;
    public string DisplayBadge => IsCustom ? "🎉 Social event" : Route?.DifficultyBadge ?? string.Empty;
    public string BookButtonText => IsCustom ? "Book tickets" : "Book this walk";

    public string DisplayDate => StartDateTime.ToString("dddd d MMMM");
    public string DisplayTime => $"{StartDateTime:HH:mm}–{EndDateTime:HH:mm}";

    public bool IsBookable => DateTime.Now >= BookingOpensAt;
    public string BookingOpensDisplay => $"Booking opens {BookingOpensAt:ddd d MMM, HH:mm}";

    /// <summary>
    /// Club policy: Monday walks open for booking the prior Thursday at
    /// 18:00; Wednesday walks open the prior Sunday at 18:00. Other days have
    /// no standard rule (null) - admins must set BookingOpensAt explicitly
    /// for those, or the walk is treated as bookable immediately.
    /// </summary>
    public static DateTime? ComputeStandardBookingOpensAt(DateTime walkStart)
    {
        return walkStart.DayOfWeek switch
        {
            DayOfWeek.Monday => walkStart.Date.AddDays(-4).AddHours(18),
            DayOfWeek.Wednesday => walkStart.Date.AddDays(-3).AddHours(18),
            _ => null
        };
    }
}
