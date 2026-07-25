using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using BoltonWalking.App.Models;
using BoltonWalking.App.ViewModels;
using MauiPin = Microsoft.Maui.Controls.Maps.Pin;

namespace BoltonWalking.App.Views;

public partial class RoutesPage : ContentPage
{
    private readonly RoutesViewModel viewModel;
    private readonly Dictionary<MauiPin, WalkingRoute> pinLookup = new();

    public RoutesPage(RoutesViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadRoutesCommand.ExecuteAsync(null);
        BuildPins();
    }

    // Pins are added manually (rather than via Map.ItemsSource) so we can
    // subscribe to each pin's MarkerClicked event and drive our own popup
    // instead of the native info window.
    private void BuildPins()
    {
        RoutesMap.Pins.Clear();
        pinLookup.Clear();

        foreach (var route in viewModel.Routes)
        {
            var pin = new MauiPin
            {
                Label = route.Name,
                Address = route.ShortDescription,
                Location = new Location(route.Latitude, route.Longitude),
                Type = PinType.Place,
                // Read by the MapPinHandler mapping in MauiProgram.cs to colour the pin by difficulty.
                BindingContext = route
            };

            pin.MarkerClicked += (s, e) =>
            {
                // Suppress the native callout - our own popup shows instead.
                e.HideInfoWindow = true;
                viewModel.PinTappedCommand.Execute(route);
            };

            pinLookup[pin] = route;
            RoutesMap.Pins.Add(pin);
        }

        if (viewModel.Routes.Count > 0)
        {
            // Fit the camera to all routes rather than just the first one,
            // since they're spread across the whole West Pennine Moors area.
            var minLat = viewModel.Routes.Min(r => r.Latitude);
            var maxLat = viewModel.Routes.Max(r => r.Latitude);
            var minLon = viewModel.Routes.Min(r => r.Longitude);
            var maxLon = viewModel.Routes.Max(r => r.Longitude);

            var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);
            var latSpan = Math.Max(maxLat - minLat, 0.02) * 1.3;
            var lonSpan = Math.Max(maxLon - minLon, 0.02) * 1.3;

            RoutesMap.MoveToRegion(new MapSpan(center, latSpan, lonSpan));
        }
    }

    private async void OnViewDetailsClicked(object? sender, EventArgs e)
    {
        if (viewModel.SelectedRoute is null) return;

        var id = viewModel.SelectedRoute.Id;
        viewModel.DismissPopupCommand.Execute(null);

        await Shell.Current.GoToAsync($"{nameof(RouteDetailsPage)}?id={id}");
    }
}
