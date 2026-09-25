using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class SubmitRoutePage : ContentPage
{
    public SubmitRoutePage(SubmitRouteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
