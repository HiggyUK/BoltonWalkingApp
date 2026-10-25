using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;

namespace BoltonWalking.App.ViewModels;

/// <summary>
/// Full write-up for a custom event (e.g. the Christmas party). Unlike
/// RouteDetailsViewModel this doesn't re-fetch anything - BookViewModel
/// hands over the already-loaded EventItem as a Shell navigation parameter.
/// </summary>
[QueryProperty(nameof(Event), EventQueryKey)]
public partial class EventDetailsViewModel : ObservableObject
{
    public const string EventQueryKey = "event";

    [ObservableProperty]
    private EventItem? @event;

    [RelayCommand]
    private async Task OpenTicketLinkAsync()
    {
        if (Event is null || !Event.IsBookable || string.IsNullOrWhiteSpace(Event.TicketLink))
            return;

        await Launcher.OpenAsync(Event.TicketLink);
    }

    // A maps search URL rather than Map.OpenAsync(Placemark) - custom event
    // addresses are free text, which a search handles far better than a
    // structured placemark. Opens the Maps/Google Maps app where installed.
    [RelayCommand]
    private async Task OpenAddressAsync()
    {
        if (Event is null) return;

        var query = string.Join(", ", new[] { Event.Venue, Event.Address }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (query.Length == 0) return;

        await Launcher.OpenAsync($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(query)}");
    }
}
