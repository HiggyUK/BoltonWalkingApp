using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IEventsService
{
    /// <summary>Events whose end time hasn't passed yet, soonest first.</summary>
    Task<List<EventItem>> GetUpcomingEventsAsync();
}
