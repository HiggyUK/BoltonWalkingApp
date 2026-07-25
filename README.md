# Bolton Walking and Outdoors Appreciation Society - App

A single .NET MAUI app for the Bolton Walking and Outdoors Appreciation Society.

## How it's structured

```
ClubsSolution.sln
└── BoltonWalking.App/
    ├── Models/          <- WalkingRoute, DownloadableFile
    ├── Services/         <- IRoutesService, IFileDownloadService (+ implementations)
    ├── ViewModels/        <- MVVM view models (CommunityToolkit.Mvvm)
    ├── Views/             <- AboutPage, BookPage, RoutesPage, RouteDetailsPage
    ├── Converters/
    ├── Resources/Styles/  <- Colors.xaml (brand palette), Styles.xaml
    ├── Resources/AppIcon/ <- real club crest (bwoas_crest.png), used for the app icon + splash
    ├── Resources/Images/  <- real photos pulled from boltonwalking.co.uk (see below)
    ├── ClubInfo.cs        <- club name/contact/booking URL constants - edit these
    └── MauiProgram.cs     <- DI registrations, branding entry point
```

Three tabs:
- **About** - hero photo, club crest, name, "Est. 2011 - Celebrating 15 years", description, email/website links.
- **Book** - opens the club's Ticket Tailor booking page in the browser (`ClubInfo.BookingUrl`), with the Monday/Wednesday booking-window note from the website.
- **Routes** - a map with pins for each walking route; tap a pin for a brief
  popup, "View details" opens a photo, the full description, and any
  downloadable `.gpx` files (via `CommunityToolkit.Maui`'s file saver).

**Branding source**: colours (`Resources/Styles/Colors.xaml`) were sampled
directly from the club crest on boltonwalking.co.uk (favicon/og:image), not
guessed. The app icon/splash use that same crest at full resolution
(`Resources/AppIcon/bwoas_crest.png`). `Resources/Images/bowas_ribbon.png` is
the anniversary-ribbon logo you provided directly, kept for future use.
`hero_about.jpg`, `route_rivington.jpg`, `route_winterhill.jpg` are real
photos from the club's own site, used as placeholder imagery until the real
18-route dataset (see below) replaces the two sample routes.

An earlier two-club version of this app (`ClubA.App`, `ClubB.App`,
`Clubs.Shared`) is kept in `_archive/` in case any of that scaffolding is
useful again - it's not part of the solution and isn't built.

## Before this actually runs for real

1. **`ClubInfo.cs`** - contact email, website, and booking URL are already
   filled in with the club's real details.
2. **Google Maps API key** - `Platforms/Android/AndroidManifest.xml` has a
   `com.google.android.geo.API_KEY` placeholder. Get a real key from the
   Google Cloud Console (Maps SDK for Android), restricted to this app's
   package name and signing certificate, or the Routes map will render blank
   (this is why the map shows no tiles/pins in the emulator right now).
3. **`ApplicationId`** in `BoltonWalking.App.csproj` (`com.boltonwalking.app`)
   is a placeholder - change it to match whatever you register in Google
   Play / App Store Connect before publishing.
4. **Real route data** - `Services/RoutesService.cs` returns two hardcoded
   sample routes (Rivington Pike, Winter Hill). The real site already has 18
   named routes with GPX/PDF downloads under "Routes and Venues" - swap the
   body of `GetRoutesAsync()`/`GetRouteAsync()` for that data (or a real API)
   when ready; nothing else needs to change.

## Getting it running

1. Open `ClubsSolution.sln` in Visual Studio 2022/2026 with the **.NET
   Multi-platform App UI** workload installed (or use the .NET MAUI VS Code
   extension / `dotnet build -t:Run -f net9.0-android`).
2. Let it restore NuGet packages and the MAUI workload targets.
3. Pick an Android emulator (or iOS simulator/device if on/connected to a
   Mac) and run.

> Note: building for iOS from Windows requires a network-paired Mac (or a Mac
> build host service) with Xcode installed - that's an Apple requirement, not
> a MAUI limitation.

## Publishing

Update `ApplicationId`, `ApplicationDisplayVersion`, `ApplicationVersion` in
`BoltonWalking.App.csproj`, and the app icons, before submitting to Google
Play / the App Store.

## Next steps worth considering

- **Backend**: a small ASP.NET Core Web API (or Azure Functions) for real
  route data, walk listings, etc.
- **Push notifications**: `Plugin.Firebase` or Azure Notification Hubs, if
  you want to notify members about new walks.
- **Membership/authentication**: ASP.NET Core Identity (self-hosted) or a
  hosted option like Auth0/Entra External ID, if members ever need to log in.
