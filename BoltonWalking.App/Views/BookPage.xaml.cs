namespace BoltonWalking.App.Views;

public partial class BookPage : ContentPage
{
    public BookPage()
    {
        InitializeComponent();
    }

    private async void OnOpenBookingPageClicked(object? sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync(ClubInfo.BookingUrl);
    }
}
