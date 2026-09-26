using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IRouteSubmissionService
{
    Task SubmitAsync(RouteSubmission submission);
}

public class RouteSubmissionService : IRouteSubmissionService
{
    private readonly FirestoreClient firestoreClient;
    private readonly FirebaseStorageClient storageClient;

    public RouteSubmissionService(FirestoreClient firestoreClient, FirebaseStorageClient storageClient)
    {
        this.firestoreClient = firestoreClient;
        this.storageClient = storageClient;
    }

    public async Task SubmitAsync(RouteSubmission submission)
    {
        // Uploaded here, at submit-time, rather than as each photo is
        // picked - so an abandoned form never leaves orphan files in
        // Storage; nothing is uploaded unless the whole submission goes
        // through.
        var photoUrls = new List<string>();
        foreach (var photo in submission.PhotosToUpload)
        {
            var path = $"submission-photos/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{photo.FileName}";
            photoUrls.Add(await storageClient.UploadAsync(path, photo.Content, photo.ContentType));
        }

        var fields = new Dictionary<string, object?>
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
            ["photoUrls"] = photoUrls.Count > 0 ? photoUrls : null,
        };

        await firestoreClient.CreateDocumentAsync("route-submissions", fields);
    }
}
