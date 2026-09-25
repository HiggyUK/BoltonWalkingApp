namespace BoltonWalking.App.Views;

public partial class FaqPage : ContentPage
{
    public FaqPage()
    {
        InitializeComponent();
    }

    private async void OnBookClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//BookPage");
    }
}
