using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class EventsPage : ContentPage
{
    private readonly EventsViewModel viewModel;

    public EventsPage(EventsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Always re-check for updates, same as Routes - not just on first visit.
        await viewModel.LoadEventsCommand.ExecuteAsync(null);
    }
}
