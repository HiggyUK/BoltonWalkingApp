using CommunityToolkit.Maui.Storage;

namespace BoltonWalking.App.Services;

public interface IFileDownloadService
{
    /// <summary>
    /// Downloads the file at <paramref name="url"/> and prompts the user to
    /// save it (Android: system save-file picker; iOS: share sheet, which
    /// includes "Save to Files" and "Open in..." any installed GPX app).
    /// </summary>
    Task DownloadAndSaveAsync(string url, string fileName);
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

    public async Task DownloadAndSaveAsync(string url, string fileName)
    {
        await using var stream = await httpClient.GetStreamAsync(url);

        var result = await fileSaver.SaveAsync(fileName, stream, CancellationToken.None);

        if (!result.IsSuccessful)
        {
            throw new IOException($"Could not save {fileName}: {result.Exception?.Message}");
        }
    }
}
