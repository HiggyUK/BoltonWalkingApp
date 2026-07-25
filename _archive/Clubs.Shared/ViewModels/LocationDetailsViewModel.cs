using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Clubs.Shared.Models;
using Clubs.Shared.Services;

namespace Clubs.Shared.ViewModels;

[QueryProperty(nameof(LocationId), "id")]
public partial class LocationDetailsViewModel : ObservableObject
{
    private readonly ILocationsService locationsService;
    private readonly IFileDownloadService fileDownloadService;

    [ObservableProperty]
    private int locationId;

    [ObservableProperty]
    private ClubLocation? location;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    public LocationDetailsViewModel(ILocationsService locationsService, IFileDownloadService fileDownloadService)
    {
        this.locationsService = locationsService;
        this.fileDownloadService = fileDownloadService;
    }

    // Fires automatically via [QueryProperty] as soon as Shell sets LocationId
    // from the "?id=" navigation parameter.
    partial void OnLocationIdChanged(int value)
    {
        _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        try
        {
            IsBusy = true;
            Location = await locationsService.GetLocationAsync(id);
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
}
