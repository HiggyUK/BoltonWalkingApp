using BoltonWalking.App.Services;
using BoltonWalking.App.Views;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace BoltonWalking.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // The app has no dark-mode palette - several controls hardcode literal
        // White/Black colours (e.g. RoutesPage's popup and search bar
        // backgrounds, AboutPage's title text) that don't move with the
        // system theme the way Label/SearchBar's own default text colour
        // does. Left alone, a device in Dark Mode gets white-on-white or
        // black-on-black text. Pin to Light until the app gets a real dark
        // theme (i.e. AppThemeBinding everywhere instead of literal colours).
        UserAppTheme = AppTheme.Light;

        LocalNotificationCenter.Current.NotificationActionTapped += OnBookingNotificationTapped;
    }

    // Tapping a "booking is now open" notification (see
    // BookingNotificationService) should take the user straight to the Book
    // tab rather than just reopening the app wherever it last was.
    private static void OnBookingNotificationTapped(NotificationActionEventArgs e)
    {
        if (e.Request.ReturningData != IBookingNotificationService.BookingOpenedReturningData) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // On a cold start (app wasn't already running) Shell.Current
            // isn't ready the instant this event fires - give it a moment.
            for (var attempt = 0; attempt < 10 && Shell.Current is null; attempt++)
                await Task.Delay(200);

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync($"//{nameof(BookPage)}");
        });
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
