using Clubs.Shared.Models;
using Clubs.Shared.Services;
using Clubs.Shared.ViewModels;
using Clubs.Shared.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace ClubB.App;

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
            ClubName = "Oakfield Cricket Club",
            ShortName = "Oakfield",
            PrimaryColorHex = "#2E7D32",
            SecondaryColorHex = "#1B4F1E",
            LogoImageResource = "oakfield_logo.png",
            ContactEmail = "secretary@oakfieldcc.org",
            WebsiteUrl = "https://oakfieldcc.org",
            MembershipPaymentsEnabled = false
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
