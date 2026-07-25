using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class CommitteePage : ContentPage
{
    private readonly CommitteeViewModel viewModel;

    public CommitteePage(CommitteeViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (viewModel.Members.Count == 0)
            await viewModel.LoadMembersCommand.ExecuteAsync(null);
    }
}
