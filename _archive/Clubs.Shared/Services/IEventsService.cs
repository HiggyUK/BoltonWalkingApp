using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

public interface IEventsService
{
    Task<List<EventItem>> GetUpcomingEventsAsync();
}
