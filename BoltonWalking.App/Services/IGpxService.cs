using Microsoft.Maui.Maps;

namespace BoltonWalking.App.Services;

public interface IGpxService
{
    /// <summary>
    /// Downloads and parses a GPX file's track points. Returns null (never
    /// throws) if the file can't be fetched or contains no usable points -
    /// callers should treat that as "no route map available" rather than an error.
    /// </summary>
    Task<List<Location>?> TryLoadTrackAsync(string gpxUrl);
}
