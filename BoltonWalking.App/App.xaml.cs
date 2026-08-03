using BoltonWalking.App.Services;
using BoltonWalking.App.Views;

namespace BoltonWalking.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Drives the app to a specific tab/page for the automated App Store
        // screenshot capture (see .github/workflows/ios-screenshots.yml).
        // Not reachable by end users - only set via `simctl launch --env`.
        var screenshotRoute = Environment.GetEnvironmentVariable("BWOAS_SCREENSHOT_ROUTE");
        if (!string.IsNullOrEmpty(screenshotRoute))
        {
            window.Created += (_, _) => _ = NavigateForScreenshotAsync(screenshotRoute);
        }

        return window;
    }

    private static async Task NavigateForScreenshotAsync(string route)
    {
        await Task.Delay(1500);

        if (route == nameof(RouteDetailsPage))
        {
            var routesService = IPlatformApplication.Current!.Services.GetRequiredService<IRoutesService>();
            var routes = await routesService.GetRoutesAsync();
            if (routes.FirstOrDefault() is { } firstRoute)
            {
                await Shell.Current.GoToAsync($"{nameof(RouteDetailsPage)}?id={firstRoute.Id}");
            }
            return;
        }

        // Committee/SafetyGuide/Faq/DifficultyGuide are pushed from the More
        // tab rather than being tabs themselves, so they need a plain (not
        // "//"-prefixed) route.
        if (route is nameof(CommitteePage) or nameof(SafetyGuidePage) or nameof(FaqPage) or nameof(DifficultyGuidePage))
        {
            await Shell.Current.GoToAsync(route);
            return;
        }

        await Shell.Current.GoToAsync($"//{route}");
    }
}
