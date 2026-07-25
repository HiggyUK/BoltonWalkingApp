using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Clubs.Shared.Models;
using Clubs.Shared.ViewModels;
using MauiPin = Microsoft.Maui.Controls.Maps.Pin;

namespace Clubs.Shared.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel viewModel;
    private readonly Dictionary<MauiPin, ClubLocation> pinLookup = new();

    public MapPage(MapViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadLocationsCommand.ExecuteAsync(null);
        BuildPins();
    }

    // Pins are added manually (rather than via Map.ItemsSource) so we can
    // subscribe to each pin's MarkerClicked event and drive our own popup
    // instead of the native info window.
    private void BuildPins()
    {
        LocationsMap.Pins.Clear();
        pinLookup.Clear();

        foreach (var location in viewModel.Locations)
        {
            var pin = new MauiPin
            {
                Label = location.Name,
                Address = location.ShortDescription,
                Location = new Location(location.Latitude, location.Longitude),
                Type = PinType.Place
            };

            pin.MarkerClicked += (s, e) =>
            {
                // Suppress the native callout - our own popup shows instead.
                e.HideInfoWindow = true;
                viewModel.PinTappedCommand.Execute(location);
            };

            pinLookup[pin] = location;
            LocationsMap.Pins.Add(pin);
        }

        if (viewModel.Locations.Count > 0)
        {
            var first = viewModel.Locations[0];
            LocationsMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(first.Latitude, first.Longitude),
                Distance.FromKilometers(5)));
        }
    }

    private async void OnViewDetailsClicked(object? sender, EventArgs e)
    {
        if (viewModel.SelectedLocation is null) return;

        var id = viewModel.SelectedLocation.Id;
        viewModel.DismissPopupCommand.Execute(null);

        await Shell.Current.GoToAsync($"{nameof(LocationDetailsPage)}?id={id}");
    }
}
