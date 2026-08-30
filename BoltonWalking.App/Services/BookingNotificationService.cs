using BoltonWalking.App.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace BoltonWalking.App.Services;

public class BookingNotificationService : IBookingNotificationService
{
    public async Task ScheduleForUpcomingEventsAsync(IEnumerable<EventItem> events)
    {
        var pending = events.Where(e => !e.IsBookable && e.BookingOpensAt > DateTime.Now).ToList();
        if (pending.Count == 0) return;

        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (!granted) return;
        }

        foreach (var eventItem in pending)
        {
            var routeName = eventItem.Route?.Name ?? "your walk";

            await LocalNotificationCenter.Current.Show(new NotificationRequest
            {
                NotificationId = StableNotificationId(eventItem.Id),
                Title = "Booking is now open",
                Description = $"Booking is now open for {routeName} on {eventItem.DisplayDate}.",
                ReturningData = IBookingNotificationService.BookingOpenedReturningData,
                Schedule = new NotificationRequestSchedule { NotifyTime = eventItem.BookingOpensAt }
            });
        }
    }

    // A stable (not string.GetHashCode - randomised per process) int id, so
    // re-scheduling the same event across app launches replaces its existing
    // notification instead of piling up duplicates.
    private static int StableNotificationId(string eventId)
    {
        unchecked
        {
            var hash = 17;
            foreach (var c in eventId)
                hash = hash * 31 + c;
            return hash & int.MaxValue;
        }
    }
}
