using BoltonWalking.App.Views;

namespace BoltonWalking.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registered explicitly because it's navigated to via GoToAsync with
        // a query parameter, rather than being one of the TabBar's items.
        Routing.RegisterRoute(nameof(RouteDetailsPage), typeof(RouteDetailsPage));
    }
}
