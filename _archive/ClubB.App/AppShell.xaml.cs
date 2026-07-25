using Clubs.Shared.Views;

namespace ClubB.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LocationDetailsPage), typeof(LocationDetailsPage));
    }
}
