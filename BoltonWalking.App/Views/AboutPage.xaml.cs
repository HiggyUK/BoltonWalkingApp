namespace BoltonWalking.App.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void OnEmailClicked(object? sender, EventArgs e)
    {
        try
        {
            await Email.Default.ComposeAsync(new EmailMessage
            {
                To = new List<string> { ClubInfo.ContactEmail }
            });
        }
        catch (Exception)
        {
            await Launcher.Default.OpenAsync($"mailto:{ClubInfo.ContactEmail}");
        }
    }

    private async void OnWebsiteClicked(object? sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync(ClubInfo.WebsiteUrl);
    }
}
