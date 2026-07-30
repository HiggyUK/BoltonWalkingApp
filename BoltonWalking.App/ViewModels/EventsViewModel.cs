using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

public partial class EventsViewModel : ObservableObject
{
    private readonly IEventsService eventsService;

    public ObservableCollection<EventItem> Events { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public EventsViewModel(IEventsService eventsService)
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
            Events.Clear();

            var items = await eventsService.GetUpcomingEventsAsync();
            foreach (var item in items)
                Events.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't load events: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenTicketLinkAsync(EventItem? eventItem)
    {
        if (eventItem is null || string.IsNullOrWhiteSpace(eventItem.TicketLink))
            return;

        await Launcher.OpenAsync(eventItem.TicketLink);
    }
}
