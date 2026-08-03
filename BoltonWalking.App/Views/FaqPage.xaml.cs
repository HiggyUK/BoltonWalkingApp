namespace BoltonWalking.App.Views;

public partial class FaqPage : ContentPage
{
    public FaqPage()
    {
        InitializeComponent();
    }

    private async void OnBookClicked(object? sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync(ClubInfo.BookingUrl);
    }
}
