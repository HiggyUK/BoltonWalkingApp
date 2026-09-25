using CommunityToolkit.Maui.Storage;

namespace BoltonWalking.App.Services;

public interface IFileDownloadService
{
    /// <summary>
    /// Downloads the file at <paramref name="url"/> and tries to open it in
    /// whatever app the OS considers appropriate for its type (Android:
    /// ACTION_VIEW - opens directly or shows a chooser; iOS: "Open in..."
    /// menu). Risk assessment PDFs almost always have a handler; GPX tracks
    /// often don't unless the user has a dedicated hiking app installed - in
    /// that case this falls back to prompting the user to save it instead
    /// (Android: system save-file picker; iOS: share sheet).
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

        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest(fileName, new ReadOnlyFile(localPath)));
        }
        catch
        {
            // No app registered to handle this file type - fall back to
            // letting the user save it themselves.
            using var stream = new MemoryStream(bytes);
            var result = await fileSaver.SaveAsync(fileName, stream, CancellationToken.None);

            if (!result.IsSuccessful)
                throw new IOException($"Could not save {fileName}: {result.Exception?.Message}");
        }
    }
}
