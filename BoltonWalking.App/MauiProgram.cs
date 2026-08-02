using BoltonWalking.App.Models;
using BoltonWalking.App.Services;
using BoltonWalking.App.ViewModels;
using BoltonWalking.App.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
#if ANDROID
using Android.Gms.Maps.Model;
#endif

namespace BoltonWalking.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseMauiCommunityToolkit();

        // Colours each route pin by difficulty (green/orange/red) to match the
        // badges shown on the route details page. RoutesPage.xaml.cs sets each
        // Pin's BindingContext to its WalkingRoute so this can read Difficulty.
#if ANDROID
        Microsoft.Maui.Maps.Handlers.MapPinHandler.Mapper.AppendToMapping("BWOAS.DifficultyColor", (handler, pinView) =>
        {
            if (pinView is Pin { BindingContext: WalkingRoute route } && handler.PlatformView is MarkerOptions markerOptions)
            {
                var hue = route.Difficulty switch
                {
                    RouteDifficulty.Easy => BitmapDescriptorFactory.HueGreen,
                    RouteDifficulty.Moderate => BitmapDescriptorFactory.HueOrange,
                    _ => BitmapDescriptorFactory.HueRed
                };
                markerOptions.InvokeIcon(BitmapDescriptorFactory.DefaultMarker(hue));
            }
        });
#endif

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<FirestoreClient>();
        builder.Services.AddSingleton<IRoutesService, RoutesService>();
        builder.Services.AddSingleton<IEventsService, EventsService>();
        builder.Services.AddSingleton<IGpxService, GpxService>();

        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
        builder.Services.AddSingleton<IFileDownloadService, FileDownloadService>();

        builder.Services.AddTransient<RoutesViewModel>();
        builder.Services.AddTransient<RouteDetailsViewModel>();
        builder.Services.AddTransient<CommitteeViewModel>();
        builder.Services.AddTransient<BookViewModel>();

        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<BookPage>();
        builder.Services.AddTransient<RoutesPage>();
        builder.Services.AddTransient<RouteDetailsPage>();
        builder.Services.AddTransient<CommitteePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
