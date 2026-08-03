using BoltonWalking.App.Views;

namespace BoltonWalking.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registered explicitly because these are pushed via GoToAsync rather
        // than being one of the TabBar's own items - RouteDetailsPage from the
        // Routes tab, Committee/SafetyGuide from the More tab's menu.
        Routing.RegisterRoute(nameof(RouteDetailsPage), typeof(RouteDetailsPage));
        Routing.RegisterRoute(nameof(CommitteePage), typeof(CommitteePage));
        Routing.RegisterRoute(nameof(SafetyGuidePage), typeof(SafetyGuidePage));
        Routing.RegisterRoute(nameof(FaqPage), typeof(FaqPage));
        Routing.RegisterRoute(nameof(DifficultyGuidePage), typeof(DifficultyGuidePage));
    }
}
