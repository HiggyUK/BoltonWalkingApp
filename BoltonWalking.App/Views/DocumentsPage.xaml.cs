using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class DocumentsPage : ContentPage
{
    private readonly DocumentsViewModel viewModel;

    public DocumentsPage(DocumentsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Always re-check, so a newly uploaded or updated policy shows up
        // without needing the Refresh button.
        await viewModel.LoadDocumentsCommand.ExecuteAsync(null);
    }
}
