using Clubs.Shared.ViewModels;

namespace Clubs.Shared.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnEventsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EventsPage));
    }
}
