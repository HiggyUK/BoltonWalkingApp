namespace BoltonWalking.App.Services;

/// <summary>
/// Minimal client for the Firebase Storage REST API - no Firebase SDK
/// dependency, just HttpClient, matching FirestoreClient's approach. Only
/// usable against paths whose Storage rules allow an unauthenticated write,
/// such as "submission-photos/" - see RouteSubmissionService.
/// </summary>
public class FirebaseStorageClient
{
    private const string Bucket = "bwoas-85868.firebasestorage.app";
    private const string BaseUrl = $"https://firebasestorage.googleapis.com/v0/b/{Bucket}/o";

    private readonly HttpClient httpClient;

    public FirebaseStorageClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>Uploads <paramref name="content"/> to <paramref name="path"/> and returns its public download URL.</summary>
    public async Task<string> UploadAsync(string path, byte[] content, string contentType)
    {
        var encodedPath = Uri.EscapeDataString(path);

        using var body = new ByteArrayContent(content);
        body.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        using var response = await httpClient.PostAsync($"{BaseUrl}?uploadType=media&name={encodedPath}", body);
        response.EnsureSuccessStatusCode();

        return $"https://firebasestorage.googleapis.com/v0/b/{Bucket}/o/{encodedPath}?alt=media";
    }
}
