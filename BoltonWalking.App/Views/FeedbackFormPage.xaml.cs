using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class FeedbackFormPage : ContentPage
{
    public FeedbackFormPage(FeedbackFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
