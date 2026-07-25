using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

/// <summary>
/// Placeholder implementation returning sample data so the UI has something
/// to bind to immediately. Replace the body of GetUpcomingEventsAsync with a
/// real HttpClient call to your backend (Azure App Service, a Google Sheet
/// via a small API, Firebase, whatever you choose) once you're ready - the
/// interface means nothing else in the app has to change.
/// </summary>
public class EventsService : IEventsService
{
    public Task<List<EventItem>> GetUpcomingEventsAsync()
    {
        var sample = new List<EventItem>
        {
            new() { Title = "Committee Meeting", StartsAt = DateTime.Today.AddDays(3).AddHours(19), Location = "Clubhouse" },
            new() { Title = "Weekend Social", StartsAt = DateTime.Today.AddDays(9).AddHours(12), Location = "Main Hall" },
        };

        return Task.FromResult(sample);
    }
}
