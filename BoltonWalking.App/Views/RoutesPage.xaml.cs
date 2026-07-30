using System.ComponentModel;
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
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.LoadRoutesCommand.ExecuteAsync(null);
        BuildPins();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(RoutesViewModel.SelectedDifficulty) or nameof(RoutesViewModel.SearchText))
            BuildPins();
    }

    // Pins are added manually (rather than via Map.ItemsSource) so we can
    // subscribe to each pin's MarkerClicked event and drive our own popup
    // instead of the native info window.
    private void BuildPins()
    {
        RoutesMap.Pins.Clear();
        pinLookup.Clear();

        var routes = viewModel.FilteredRoutes.ToList();
        NoResultsLabel.IsVisible = routes.Count == 0 && viewModel.Routes.Count > 0;

        foreach (var route in routes)
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

        if (routes.Count > 0)
        {
            // Fit the camera to the filtered routes rather than just the
            // first one, since they're spread across the whole West
            // Pennine Moors area.
            var minLat = routes.Min(r => r.Latitude);
            var maxLat = routes.Max(r => r.Latitude);
            var minLon = routes.Min(r => r.Longitude);
            var maxLon = routes.Max(r => r.Longitude);

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
