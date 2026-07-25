using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Clubs.Shared.Models;
using Clubs.Shared.Services;

namespace Clubs.Shared.ViewModels;

public partial class EventsViewModel : ObservableObject
{
    private readonly IEventsService eventsService;

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<EventItem> Events { get; } = new();

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
            Events.Clear();

            var items = await eventsService.GetUpcomingEventsAsync();
            foreach (var item in items)
                Events.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
