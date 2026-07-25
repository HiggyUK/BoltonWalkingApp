using Clubs.Shared.ViewModels;

namespace Clubs.Shared.Views;

public partial class LocationDetailsPage : ContentPage
{
    public LocationDetailsPage(LocationDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
