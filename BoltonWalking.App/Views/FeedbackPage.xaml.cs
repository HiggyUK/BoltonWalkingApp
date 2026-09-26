using BoltonWalking.App.Models;

namespace BoltonWalking.App.Views;

public partial class FeedbackPage : ContentPage
{
    public FeedbackPage()
    {
        InitializeComponent();
    }

    private async void OnGeneralTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(FeedbackFormPage)}?category={nameof(FeedbackCategory.General)}");
    }

    private async void OnAppTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(FeedbackFormPage)}?category={nameof(FeedbackCategory.App)}");
    }

    private async void OnRouteWalkTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(FeedbackFormPage)}?category={nameof(FeedbackCategory.RouteWalk)}");
    }

    private async void OnSubmitRouteTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SubmitRoutePage));
    }
}
