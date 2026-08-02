using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Maps;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

[QueryProperty(nameof(RouteId), "id")]
public partial class RouteDetailsViewModel : ObservableObject
{
    private readonly IRoutesService routesService;
    private readonly IFileDownloadService fileDownloadService;
    private readonly IGpxService gpxService;

    [ObservableProperty]
    private int routeId;

    [ObservableProperty]
    private WalkingRoute? route;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    // Populated from the route's .gpx file, if it has one - see LoadRouteTrackAsync.
    public ObservableCollection<Location> RouteTrack { get; } = new();

    // True once RouteTrack has real points - RouteDetailsPage uses this to
    // know when it's safe to build the map polyline and to show/hide the
    // embedded map section.
    [ObservableProperty]
    private bool hasRouteTrack;

    public RouteDetailsViewModel(IRoutesService routesService, IFileDownloadService fileDownloadService, IGpxService gpxService)
    {
        this.routesService = routesService;
        this.fileDownloadService = fileDownloadService;
        this.gpxService = gpxService;
    }

    // Fires automatically via [QueryProperty] as soon as Shell sets RouteId
    // from the "?id=" navigation parameter.
    partial void OnRouteIdChanged(int value)
    {
        _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        try
        {
            IsBusy = true;
            Route = await routesService.GetRouteAsync(id);
        }
        finally
        {
            IsBusy = false;
        }

        await LoadRouteTrackAsync();
    }

    private async Task LoadRouteTrackAsync()
    {
        RouteTrack.Clear();
        HasRouteTrack = false;

        var gpxFile = Route?.Files.FirstOrDefault(f => f.FileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase));
        if (gpxFile is null) return;

        var points = await gpxService.TryLoadTrackAsync(gpxFile.Url);
        if (points is null || points.Count == 0) return;

        foreach (var point in points)
            RouteTrack.Add(point);

        // Set last, after RouteTrack is fully populated - RouteDetailsPage
        // builds the polyline in response to this flipping to true.
        HasRouteTrack = true;
    }

    [RelayCommand]
    private async Task DownloadFileAsync(DownloadableFile file)
    {
        try
        {
            StatusMessage = $"Downloading {file.FileName}...";
            await fileDownloadService.DownloadAndSaveAsync(file.Url, file.FileName);
            StatusMessage = $"Saved {file.FileName}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't download {file.FileName}: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task OpenWhat3WordsAsync()
    {
        if (Route is null || string.IsNullOrWhiteSpace(Route.What3Words)) return;

        await Launcher.Default.OpenAsync($"https://w3w.co/{Route.What3Words}");
    }
}
