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

    // When booking opens for this walk. Missing/unset in Firestore is treated
    // as DateTime.MinValue (i.e. always bookable) rather than locking the
    // walk out - see EventsService.
    public DateTime BookingOpensAt { get; set; }

    // Populated by EventsService after fetching both collections - null if
    // the route was deleted/renumbered after the event was created.
    public WalkingRoute? Route { get; set; }

    public string DisplayDate => StartDateTime.ToString("dddd d MMMM");
    public string DisplayTime => $"{StartDateTime:HH:mm}–{EndDateTime:HH:mm}";

    public bool IsBookable => DateTime.Now >= BookingOpensAt;
    public string BookingOpensDisplay => $"Booking opens {BookingOpensAt:ddd d MMM, HH:mm}";
}
