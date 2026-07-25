using Clubs.Shared.Views;

namespace ClubA.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registered explicitly because it's navigated to via GoToAsync with
        // a query parameter, rather than being one of the TabBar's items.
        Routing.RegisterRoute(nameof(LocationDetailsPage), typeof(LocationDetailsPage));
    }
}
