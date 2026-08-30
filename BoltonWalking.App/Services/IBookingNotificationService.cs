using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IBookingNotificationService
{
    // Tagged on every notification's ReturningData so the tap handler (see
    // App.xaml.cs) can tell it's one of ours and route to the Book tab.
    const string BookingOpenedReturningData = "booking-opened";

    /// <summary>
    /// Schedules a local notification for each event whose booking hasn't
    /// opened yet, to fire at its BookingOpensAt. Safe to call repeatedly
    /// (e.g. every time the Book page loads) - re-scheduling the same event
    /// replaces its existing notification rather than duplicating it.
    /// </summary>
    Task ScheduleForUpcomingEventsAsync(IEnumerable<EventItem> events);
}
