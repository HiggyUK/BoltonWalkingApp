using System.Text.Json;
using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public class EventsService : IEventsService
{
    private readonly FirestoreClient firestoreClient;
    private readonly IRoutesService routesService;

    public EventsService(FirestoreClient firestoreClient, IRoutesService routesService)
    {
        this.firestoreClient = firestoreClient;
        this.routesService = routesService;
    }

    public async Task<List<EventItem>> GetUpcomingEventsAsync()
    {
        var events = await GetAllEventsAsync();
        return events
            .Where(e => e.EndDateTime >= DateTime.Now)
            .OrderBy(e => e.StartDateTime)
            .ToList();
    }

    public async Task<List<EventItem>> GetPastEventsAsync()
    {
        var events = await GetAllEventsAsync();
        return events
            .Where(e => e.EndDateTime < DateTime.Now)
            .OrderByDescending(e => e.StartDateTime)
            .ToList();
    }

    private async Task<List<EventItem>> GetAllEventsAsync()
    {
        var routesTask = routesService.GetRoutesAsync();
        var documents = await firestoreClient.GetCollectionAsync("events");
        var routesById = (await routesTask).ToDictionary(r => r.Id);

        return documents.Select(doc => MapEvent(doc, routesById)).ToList();
    }

    private static EventItem MapEvent((string Id, JsonElement Fields) document, Dictionary<int, WalkingRoute> routesById)
    {
        var (id, fields) = document;
        var routeId = (int)FirestoreClient.GetDouble(fields, "routeId");
        routesById.TryGetValue(routeId, out var route);

        var start = DateTime.TryParse(FirestoreClient.GetString(fields, "startDateTime"), out var s) ? s : DateTime.MinValue;

        // An explicit, real bookingOpensAt overrides the standard rule (for
        // exceptions). Anything else - missing, unparseable, or the "not
        // set" sentinel written by the admin tools when left blank - falls
        // back to the club's Monday/Wednesday rule computed from the walk's
        // own date, and only becomes "always bookable" if that rule doesn't
        // apply either (i.e. the walk isn't on a Monday or Wednesday).
        var explicitOpens = DateTime.TryParse(FirestoreClient.GetString(fields, "bookingOpensAt"), out var o) ? o : (DateTime?)null;
        var bookingOpensAt = explicitOpens is { } real && real > DateTime.MinValue
            ? real
            : EventItem.ComputeStandardBookingOpensAt(start) ?? DateTime.MinValue;

        return new EventItem
        {
            Id = id,
            RouteId = routeId,
            StartDateTime = start,
            EndDateTime = DateTime.TryParse(FirestoreClient.GetString(fields, "endDateTime"), out var end) ? end : DateTime.MinValue,
            TicketLink = FirestoreClient.GetString(fields, "ticketLink"),
            BookingOpensAt = bookingOpensAt,
            Route = route
        };
    }
}
