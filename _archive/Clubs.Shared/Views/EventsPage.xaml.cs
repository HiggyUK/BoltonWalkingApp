using Clubs.Shared.ViewModels;

namespace Clubs.Shared.Views;

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
        await viewModel.LoadEventsCommand.ExecuteAsync(null);
    }
}
