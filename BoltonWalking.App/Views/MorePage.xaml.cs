namespace BoltonWalking.App.Views;

public partial class MorePage : ContentPage
{
    public MorePage()
    {
        InitializeComponent();
    }

    private async void OnFaqTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(FaqPage));
    }

    private async void OnSafetyGuideTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SafetyGuidePage));
    }

    private async void OnDifficultyGuideTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DifficultyGuidePage));
    }

    private async void OnCommitteeTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CommitteePage));
    }
}
