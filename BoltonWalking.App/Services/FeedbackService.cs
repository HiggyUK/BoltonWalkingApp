using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IFeedbackService
{
    Task SubmitAsync(FeedbackSubmission feedback);
}

public class FeedbackService : IFeedbackService
{
    private readonly FirestoreClient firestoreClient;

    public FeedbackService(FirestoreClient firestoreClient)
    {
        this.firestoreClient = firestoreClient;
    }

    public Task SubmitAsync(FeedbackSubmission feedback)
    {
        var fields = new Dictionary<string, object?>
        {
            ["category"] = feedback.Category.ToString(),
            ["submitterName"] = feedback.SubmitterName,
            ["submitterEmail"] = feedback.SubmitterEmail,
            ["rating"] = feedback.Rating,
            ["comments"] = feedback.Comments,
            ["appFeedbackType"] = feedback.AppFeedbackType,
            ["devicePlatform"] = DeviceInfo.Platform.ToString(),
            ["deviceModel"] = DeviceInfo.Model,
            ["eventId"] = feedback.EventId,
            ["eventLabel"] = feedback.EventLabel,
            ["walkQuality"] = feedback.WalkQuality,
            ["walkOrganisation"] = feedback.WalkOrganisation,
            ["bookingExperience"] = feedback.BookingExperience,
        };

        return firestoreClient.CreateDocumentAsync("feedback", fields);
    }
}
