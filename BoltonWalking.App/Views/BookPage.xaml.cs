using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class BookPage : ContentPage
{
    private readonly BookViewModel viewModel;

    public BookPage(BookViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Always re-check for updates, same as Routes.
        await viewModel.LoadEventsCommand.ExecuteAsync(null);
    }
}
