using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

public partial class RoutesViewModel : ObservableObject
{
    private readonly IRoutesService routesService;

    public ObservableCollection<WalkingRoute> Routes { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    // The route currently shown in the brief-info popup, or null if none is shown.
    [ObservableProperty]
    private WalkingRoute? selectedRoute;

    // Kept alongside SelectedRoute purely so XAML can bind IsVisible without a converter.
    [ObservableProperty]
    private bool isPopupVisible;

    // "All", "Easy", "Moderate", or "Hard" - which pins RoutesPage.BuildPins() should show.
    [ObservableProperty]
    private string selectedDifficulty = "All";

    // Free-text search against route name and venue - combined with the
    // difficulty filter (a route must match both).
    [ObservableProperty]
    private string searchText = "";

    public RoutesViewModel(IRoutesService routesService)
    {
        this.routesService = routesService;
    }

    [RelayCommand]
    private void FilterByDifficulty(string difficulty)
    {
        SelectedDifficulty = difficulty;
    }

    public IEnumerable<WalkingRoute> FilteredRoutes
    {
        get
        {
            var byDifficulty = SelectedDifficulty switch
            {
                "Easy" => Routes.Where(r => r.Difficulty == RouteDifficulty.Easy),
                "Moderate" => Routes.Where(r => r.Difficulty == RouteDifficulty.Moderate),
                "Hard" => Routes.Where(r => r.Difficulty == RouteDifficulty.Hard),
                _ => Routes.AsEnumerable()
            };

            var search = SearchText.Trim();
            if (search.Length == 0)
                return byDifficulty;

            return byDifficulty.Where(r =>
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Venue.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
    }

    [RelayCommand]
    private async Task LoadRoutesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            var items = await routesService.GetRoutesAsync();
            Routes.Clear();
            foreach (var item in items)
                Routes.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server - showing the last routes loaded. ({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Called when a pin is tapped - shows the brief popup for that route.
    [RelayCommand]
    private void PinTapped(WalkingRoute route)
    {
        SelectedRoute = route;
        IsPopupVisible = true;
    }

    // Called when the popup is dismissed without navigating onward.
    [RelayCommand]
    private void DismissPopup()
    {
        IsPopupVisible = false;
        SelectedRoute = null;
    }
}
