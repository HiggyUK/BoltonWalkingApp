using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

[QueryProperty(nameof(Category), "category")]
public partial class FeedbackFormViewModel : ObservableObject
{
    private readonly IFeedbackService feedbackService;
    private readonly IEventsService eventsService;

    // Raw string from the "?category=" route parameter (Shell's automatic
    // query-property conversion isn't guaranteed for enums, unlike the int
    // conversion RouteDetailsViewModel relies on for "?id=" - so this stays
    // a string and CategoryEnum below parses it).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategoryEnum))]
    [NotifyPropertyChangedFor(nameof(IsGeneral))]
    [NotifyPropertyChangedFor(nameof(IsApp))]
    [NotifyPropertyChangedFor(nameof(IsRouteWalk))]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string category = nameof(FeedbackCategory.General);

    public FeedbackCategory CategoryEnum =>
        Enum.TryParse<FeedbackCategory>(Category, out var value) ? value : FeedbackCategory.General;

    public bool IsGeneral => CategoryEnum == FeedbackCategory.General;
    public bool IsApp => CategoryEnum == FeedbackCategory.App;
    public bool IsRouteWalk => CategoryEnum == FeedbackCategory.RouteWalk;

    public string Title => CategoryEnum switch
    {
        FeedbackCategory.App => "App Feedback",
        FeedbackCategory.RouteWalk => "Route/Walk Feedback",
        _ => "General Feedback"
    };

    [ObservableProperty]
    private string submitterName = "";

    [ObservableProperty]
    private string submitterEmail = "";

    [ObservableProperty]
    private int? rating;

    [ObservableProperty]
    private string? comments;

    // App feedback only.
    public List<string> AppFeedbackTypes { get; } = new() { "Bug report", "Suggestion", "Something else" };

    [ObservableProperty]
    private string? appFeedbackType;

    // Route/walk feedback only. Labels are pre-formatted ("route - date")
    // and kept in lockstep with PastEvents so the Picker can bind to plain
    // strings (no ItemDisplayBinding juggling) while SelectedEventIndex
    // maps back to the real EventItem at submit time.
    public ObservableCollection<EventItem> PastEvents { get; } = new();
    public ObservableCollection<string> PastEventLabels { get; } = new();

    [ObservableProperty]
    private int selectedEventIndex = -1;

    [ObservableProperty]
    private string? walkQuality;

    [ObservableProperty]
    private string? walkOrganisation;

    [ObservableProperty]
    private string? bookingExperience;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    public List<int> RatingOptions { get; } = new() { 1, 2, 3, 4, 5 };

    public FeedbackFormViewModel(IFeedbackService feedbackService, IEventsService eventsService)
    {
        this.feedbackService = feedbackService;
        this.eventsService = eventsService;
    }

    partial void OnCategoryChanged(string value)
    {
        if (IsRouteWalk && PastEvents.Count == 0)
            _ = LoadPastEventsAsync();
    }

    private async Task LoadPastEventsAsync()
    {
        try
        {
            var events = await eventsService.GetPastEventsAsync();
            PastEvents.Clear();
            PastEventLabels.Clear();
            foreach (var e in events)
            {
                PastEvents.Add(e);
                PastEventLabels.Add($"{e.Route?.Name ?? "Unknown route"} - {e.DisplayDate}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't load past walks: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(SubmitterName) || string.IsNullOrWhiteSpace(SubmitterEmail))
        {
            StatusMessage = "Please fill in your name and email.";
            return;
        }

        var selectedEvent = IsRouteWalk && SelectedEventIndex >= 0 && SelectedEventIndex < PastEvents.Count
            ? PastEvents[SelectedEventIndex]
            : null;

        if (IsRouteWalk && selectedEvent is null)
        {
            StatusMessage = "Please pick which walk this feedback is about.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            var feedback = new FeedbackSubmission
            {
                Category = CategoryEnum,
                SubmitterName = SubmitterName,
                SubmitterEmail = SubmitterEmail,
                Rating = Rating,
                Comments = Comments,
                AppFeedbackType = IsApp ? AppFeedbackType : null,
                EventId = selectedEvent?.Id,
                EventLabel = IsRouteWalk && selectedEvent is not null
                    ? PastEventLabels[SelectedEventIndex]
                    : null,
                WalkQuality = IsRouteWalk ? WalkQuality : null,
                WalkOrganisation = IsRouteWalk ? WalkOrganisation : null,
                BookingExperience = IsRouteWalk ? BookingExperience : null,
            };

            await feedbackService.SubmitAsync(feedback);

            await Shell.Current.DisplayAlertAsync(
                "Thanks!",
                "Your feedback has been sent to the committee.",
                "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't send your feedback: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
