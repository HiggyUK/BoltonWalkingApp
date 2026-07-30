using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

public partial class BookViewModel : ObservableObject
{
    private readonly IEventsService eventsService;

    public ObservableCollection<EventItem> Events { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public BookViewModel(IEventsService eventsService)
    {
        this.eventsService = eventsService;
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var items = await eventsService.GetUpcomingEventsAsync();
            Events.Clear();
            foreach (var item in items)
                Events.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server - showing the last walks loaded. ({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenTicketLinkAsync(EventItem? eventItem)
    {
        // The button is disabled until IsBookable, but guard here too in case
        // it's ever reachable another way.
        if (eventItem is null || !eventItem.IsBookable || string.IsNullOrWhiteSpace(eventItem.TicketLink))
            return;

        await Launcher.OpenAsync(eventItem.TicketLink);
    }

    [RelayCommand]
    private async Task OpenBookingPageAsync()
    {
        await Launcher.OpenAsync(ClubInfo.BookingUrl);
    }
}
