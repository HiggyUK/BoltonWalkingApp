namespace BoltonWalking.App.Models;

/// <summary>
/// A scheduled walk - links a WalkingRoute to a date/time and a Ticket
/// Tailor booking link. Read from the "events" Firestore collection and
/// joined to its route client-side (see EventsService).
/// </summary>
public class EventItem
{
    public string Id { get; set; } = string.Empty;
    public int RouteId { get; set; }
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
