using CommunityToolkit.Maui.Storage;

namespace BoltonWalking.App.Services;

public interface IFileDownloadService
{
    /// <summary>
    /// Downloads the file at <paramref name="url"/> and opens it in an app
    /// that handles its type, if the phone has one (Android: that app, or a
    /// chooser if several; iOS: "Open in..." menu). Risk assessment PDFs
    /// almost always have a handler; GPX tracks only do if a hiking/mapping
    /// app is installed - otherwise the user is prompted to save the file
    /// instead (Android: system save-file picker; iOS: the "Open in..." menu
    /// already includes Save to Files).
    /// </summary>
    Task OpenOrSaveAsync(string url, string fileName);
}

public class FileDownloadService : IFileDownloadService
{
    private readonly HttpClient httpClient;
    private readonly IFileSaver fileSaver;

    public FileDownloadService(HttpClient httpClient, IFileSaver fileSaver)
    {
        this.httpClient = httpClient;
        this.fileSaver = fileSaver;
    }

    public async Task OpenOrSaveAsync(string url, string fileName)
    {
        var bytes = await httpClient.GetByteArrayAsync(url);

        // Cache dir rather than the save-picker location - the OS's own
        // "open with" chooser needs to read this straight back off disk.
        var localPath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(localPath, bytes);

        // A real content type matters on Android: without one the file goes
        // out as application/octet-stream, which apps like Google Wallet claim
        // and then silently close - so a GPX tap appeared to do nothing.
        var contentType = ContentTypeFor(fileName);

        if (HasAppToOpen(fileName, contentType))
        {
            try
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest(fileName, new ReadOnlyFile(localPath, contentType)));
                return;
            }
            catch
            {
                // Fall through to saving it instead.
            }
        }

        using var stream = new MemoryStream(bytes);
        var result = await fileSaver.SaveAsync(fileName, stream, CancellationToken.None);

        if (!result.IsSuccessful)
            throw new IOException($"Could not save {fileName}: {result.Exception?.Message}");
    }

    private static string ContentTypeFor(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".gpx" => "application/gpx+xml",
            _ => "application/octet-stream"
        };

    /// <summary>
    /// Android: whether any installed app can view this type - checked up
    /// front because Launcher.OpenAsync doesn't throw when nothing can (it
    /// shows an empty chooser instead), so it can't be relied on to trigger
    /// the save fallback. Needs a matching &lt;queries&gt; entry in
    /// AndroidManifest.xml for each type, or Android 11+ hides the apps.
    /// iOS: always true - its "Open in..." menu includes Save to Files.
    /// </summary>
    private static bool HasAppToOpen(string fileName, string contentType)
    {
#if ANDROID
        var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
        intent.SetDataAndType(Android.Net.Uri.Parse($"content://{AppInfo.Current.PackageName}.fileProvider/{Uri.EscapeDataString(fileName)}"), contentType);

        var packageManager = Platform.AppContext.PackageManager;
        if (packageManager is null) return false;

#pragma warning disable CA1422 // The flags overload is deprecated on Android 13+ but still works and covers older versions.
        var handlers = packageManager.QueryIntentActivities(intent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
#pragma warning restore CA1422
        return handlers.Count > 0;
#else
        return true;
#endif
    }
}
