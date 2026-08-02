using System.Globalization;
using System.Xml.Linq;
using Microsoft.Maui.Maps;

namespace BoltonWalking.App.Services;

public class GpxService : IGpxService
{
    // A very detailed GPX export can have several thousand points - more
    // than a small embedded preview map needs. Downsampled to keep the
    // polyline light without visibly changing its shape.
    private const int MaxPoints = 1500;

    private readonly HttpClient httpClient;

    public GpxService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<Location>?> TryLoadTrackAsync(string gpxUrl)
    {
        try
        {
            var gpxText = await httpClient.GetStringAsync(gpxUrl);
            var doc = XDocument.Parse(gpxText);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            // Most GPX exports use <trk><trkseg><trkpt>; a few use a plain
            // <rte><rtept> route instead - fall back to that if no track points exist.
            var points = ParsePoints(doc, ns, "trkpt");
            if (points.Count == 0)
                points = ParsePoints(doc, ns, "rtept");

            if (points.Count == 0)
                return null;

            if (points.Count > MaxPoints)
            {
                var step = (int)Math.Ceiling(points.Count / (double)MaxPoints);
                points = points.Where((_, i) => i % step == 0).ToList();
            }

            return points;
        }
        catch
        {
            // Network failure, malformed XML, unexpected structure - all
            // treated the same: no route map for this route, not a crash.
            return null;
        }
    }

    private static List<Location> ParsePoints(XDocument doc, XNamespace ns, string elementName)
    {
        return doc.Descendants(ns + elementName)
            .Select(ParsePoint)
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();
    }

    private static Location? ParsePoint(XElement element)
    {
        var latText = element.Attribute("lat")?.Value;
        var lonText = element.Attribute("lon")?.Value;

        if (latText is not null && lonText is not null &&
            double.TryParse(latText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(lonText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
        {
            return new Location(lat, lon);
        }

        return null;
    }
}
