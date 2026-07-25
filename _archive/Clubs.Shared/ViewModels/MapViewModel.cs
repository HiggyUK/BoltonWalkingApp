using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Clubs.Shared.Models;
using Clubs.Shared.Services;

namespace Clubs.Shared.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly ILocationsService locationsService;

    public ObservableCollection<ClubLocation> Locations { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    // The location currently shown in the brief-info popup, or null if none is shown.
    [ObservableProperty]
    private ClubLocation? selectedLocation;

    // Kept alongside SelectedLocation purely so XAML can bind IsVisible without a converter.
    [ObservableProperty]
    private bool isPopupVisible;

    public MapViewModel(ILocationsService locationsService)
    {
        this.locationsService = locationsService;
    }

    [RelayCommand]
    private async Task LoadLocationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Locations.Clear();

            var items = await locationsService.GetLocationsAsync();
            foreach (var item in items)
                Locations.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Called when a pin is tapped - shows the brief popup for that location.
    [RelayCommand]
    private void PinTapped(ClubLocation location)
    {
        SelectedLocation = location;
        IsPopupVisible = true;
    }

    // Called when the popup is dismissed without navigating onward.
    [RelayCommand]
    private void DismissPopup()
    {
        IsPopupVisible = false;
        SelectedLocation = null;
    }
}
