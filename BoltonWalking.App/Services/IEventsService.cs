using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IEventsService
{
    /// <summary>Events whose end time hasn't passed yet, soonest first.</summary>
    Task<List<EventItem>> GetUpcomingEventsAsync();

    /// <summary>Events whose end time has already passed, most recent first - for Route/Walk feedback's "pick a walk".</summary>
    Task<List<EventItem>> GetPastEventsAsync();
}
