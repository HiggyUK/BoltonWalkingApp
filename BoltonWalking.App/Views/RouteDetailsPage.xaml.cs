using System.ComponentModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class RouteDetailsPage : ContentPage
{
    private IDispatcherTimer? carouselTimer;
    private readonly RouteDetailsViewModel viewModel;

    public RouteDetailsPage(RouteDetailsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        carouselTimer = Dispatcher.CreateTimer();
        carouselTimer.Interval = TimeSpan.FromSeconds(3.5);
        carouselTimer.Tick += OnCarouselTick;
        carouselTimer.Start();

        // Covers the case where the GPX track finished loading before this
        // page appeared (the PropertyChanged subscription below covers the
        // more common case where it finishes after).
        if (viewModel.HasRouteTrack)
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), BuildRouteLine);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (carouselTimer is null) return;
        carouselTimer.Stop();
        carouselTimer.Tick -= OnCarouselTick;
        carouselTimer = null;
    }

    private void OnCarouselTick(object? sender, EventArgs e)
    {
        var count = PhotoCarousel.ItemsSource is ICollection<string> items ? items.Count : 0;
        if (count == 0) return;

        PhotoCarousel.Position = (PhotoCarousel.Position + 1) % count;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The map's Grid becomes visible at the same moment HasRouteTrack
        // flips true. Building the polyline immediately races Android's
        // layout pass - the native map view can still be zero-sized when
        // MoveToRegion runs, leaving it stuck on the placeholder tile grid.
        // A short delay lets that layout pass complete first.
        if (e.PropertyName == nameof(RouteDetailsViewModel.HasRouteTrack) && viewModel.HasRouteTrack)
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), BuildRouteLine);
    }

    private void BuildRouteLine()
    {
        var track = viewModel.RouteTrack;
        if (track.Count == 0) return;

        RouteMap.MapElements.Clear();

        var polyline = new Polyline
        {
            StrokeColor = Color.FromArgb("#E0232B"),
            StrokeWidth = 4
        };
        foreach (var point in track)
            polyline.Geopath.Add(point);

        RouteMap.MapElements.Add(polyline);

        var minLat = track.Min(p => p.Latitude);
        var maxLat = track.Max(p => p.Latitude);
        var minLon = track.Min(p => p.Longitude);
        var maxLon = track.Max(p => p.Longitude);

        var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);
        var latSpan = Math.Max(maxLat - minLat, 0.01) * 1.3;
        var lonSpan = Math.Max(maxLon - minLon, 0.01) * 1.3;

        RouteMap.MoveToRegion(new MapSpan(center, latSpan, lonSpan));
    }
}
