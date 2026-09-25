using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IRouteSubmissionService
{
    Task SubmitAsync(RouteSubmission submission);
}

public class RouteSubmissionService : IRouteSubmissionService
{
    private readonly FirestoreClient firestoreClient;

    public RouteSubmissionService(FirestoreClient firestoreClient)
    {
        this.firestoreClient = firestoreClient;
    }

    public Task SubmitAsync(RouteSubmission submission)
    {
        var fields = new Dictionary<string, string?>
        {
            ["submitterName"] = submission.SubmitterName,
            ["submitterEmail"] = submission.SubmitterEmail,
            ["name"] = submission.RouteName,
            ["venue"] = submission.Venue,
            ["difficulty"] = submission.Difficulty.ToString(),
            ["shortDescription"] = submission.ShortDescription,
            ["terrainNotes"] = submission.TerrainNotes,
            ["gpxFileName"] = submission.GpxFileName,
            ["gpxContentBase64"] = submission.GpxContentBase64,
            ["routeUrl"] = submission.RouteUrl,
        };

        return firestoreClient.CreateDocumentAsync("route-submissions", fields);
    }
}
