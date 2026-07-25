using Clubs.Shared.Models;
using Clubs.Shared.Services;
using Clubs.Shared.ViewModels;
using Clubs.Shared.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace ClubA.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ---- This block is the entire difference between ClubA.App and ClubB.App ----
        var clubConfig = new ClubConfig
        {
            ClubName = "Riverside Rowing Club",
            ShortName = "Riverside",
            PrimaryColorHex = "#0B5FA5",
            SecondaryColorHex = "#083C6B",
            LogoImageResource = "riverside_logo.png",
            ContactEmail = "info@riversiderowing.org",
            WebsiteUrl = "https://riversiderowing.org",
            MembershipPaymentsEnabled = true
        };
        // --------------------------------------------------------------------------

        builder.Services.AddSingleton(clubConfig);
        builder.Services.AddSingleton<IClubConfigService, ClubConfigService>();
        builder.Services.AddSingleton<IEventsService, EventsService>();
        builder.Services.AddSingleton<ILocationsService, LocationsService>();

        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IFileDownloadService, FileDownloadService>();

        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<EventsViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<LocationDetailsViewModel>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<EventsPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<LocationDetailsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
