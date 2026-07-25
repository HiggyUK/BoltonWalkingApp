using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

[QueryProperty(nameof(RouteId), "id")]
public partial class RouteDetailsViewModel : ObservableObject
{
    private readonly IRoutesService routesService;
    private readonly IFileDownloadService fileDownloadService;

    [ObservableProperty]
    private int routeId;

    [ObservableProperty]
    private WalkingRoute? route;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    public RouteDetailsViewModel(IRoutesService routesService, IFileDownloadService fileDownloadService)
    {
        this.routesService = routesService;
        this.fileDownloadService = fileDownloadService;
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
