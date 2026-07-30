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
        var routesTask = routesService.GetRoutesAsync();
        var documents = await firestoreClient.GetCollectionAsync("events");
        var routesById = (await routesTask).ToDictionary(r => r.Id);

        return documents
            .Select(doc => MapEvent(doc, routesById))
            .Where(e => e.EndDateTime >= DateTime.Now)
            .OrderBy(e => e.StartDateTime)
            .ToList();
    }

    private static EventItem MapEvent((string Id, JsonElement Fields) document, Dictionary<int, WalkingRoute> routesById)
    {
        var (id, fields) = document;
        var routeId = (int)FirestoreClient.GetDouble(fields, "routeId");
        routesById.TryGetValue(routeId, out var route);

        return new EventItem
        {
            Id = id,
            RouteId = routeId,
            StartDateTime = DateTime.TryParse(FirestoreClient.GetString(fields, "startDateTime"), out var start) ? start : DateTime.MinValue,
            EndDateTime = DateTime.TryParse(FirestoreClient.GetString(fields, "endDateTime"), out var end) ? end : DateTime.MinValue,
            TicketLink = FirestoreClient.GetString(fields, "ticketLink"),
            // Missing/unparseable = always bookable, rather than locking out
            // events created before this field existed.
            BookingOpensAt = DateTime.TryParse(FirestoreClient.GetString(fields, "bookingOpensAt"), out var opens) ? opens : DateTime.MinValue,
            Route = route
        };
    }
}
