using CommunityToolkit.Mvvm.ComponentModel;
using Clubs.Shared.Services;

namespace Clubs.Shared.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string clubName = string.Empty;

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    public HomeViewModel(IClubConfigService clubConfigService)
    {
        var club = clubConfigService.Current;
        ClubName = club.ClubName;
        WelcomeMessage = $"Welcome to {club.ClubName}!";
    }
}
