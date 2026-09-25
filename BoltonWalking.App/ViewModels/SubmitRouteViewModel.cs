using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

public partial class SubmitRouteViewModel : ObservableObject
{
    private readonly IRouteSubmissionService submissionService;

    private byte[]? gpxBytes;

    [ObservableProperty]
    private string? submitterName;

    [ObservableProperty]
    private string? submitterEmail;

    [ObservableProperty]
    private string routeName = "";

    [ObservableProperty]
    private string venue = "";

    [ObservableProperty]
    private RouteDifficulty difficulty = RouteDifficulty.Moderate;

    [ObservableProperty]
    private string? shortDescription;

    [ObservableProperty]
    private string? terrainNotes;

    // "...or paste a route URL" - only one of this or a picked GPX is required.
    [ObservableProperty]
    private string? routeUrl;

    // Filename of the picked GPX, shown next to the "Pick GPX file" button -
    // null until PickGpxCommand succeeds.
    [ObservableProperty]
    private string? gpxFileName;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    public List<RouteDifficulty> DifficultyOptions { get; } =
        Enum.GetValues<RouteDifficulty>().ToList();

    public SubmitRouteViewModel(IRouteSubmissionService submissionService)
    {
        this.submissionService = submissionService;
    }

    [RelayCommand]
    private async Task PickGpxAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result is null) return;

            if (!result.FileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "That's not a .gpx file - please pick a GPX track.";
                return;
            }

            await using var stream = await result.OpenReadAsync();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            gpxBytes = memory.ToArray();

            GpxFileName = result.FileName;
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't read that file: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(RouteName) || string.IsNullOrWhiteSpace(Venue))
        {
            StatusMessage = "Please fill in the route name and starting venue.";
            return;
        }

        if (gpxBytes is null && string.IsNullOrWhiteSpace(RouteUrl))
        {
            StatusMessage = "Please attach a GPX file, or paste a route URL if you don't have one.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var submission = new RouteSubmission
            {
                SubmitterName = SubmitterName,
                SubmitterEmail = SubmitterEmail,
                RouteName = RouteName,
                Venue = Venue,
                Difficulty = Difficulty,
                ShortDescription = ShortDescription,
                TerrainNotes = TerrainNotes,
                GpxFileName = GpxFileName,
                GpxContentBase64 = gpxBytes is null ? null : Convert.ToBase64String(gpxBytes),
                RouteUrl = RouteUrl,
            };

            await submissionService.SubmitAsync(submission);

            await Shell.Current.DisplayAlertAsync(
                "Thanks!",
                "Your route has been sent to the committee for review.",
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't submit your route: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
