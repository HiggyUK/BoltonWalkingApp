using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

/// <summary>
/// Placeholder implementation with sample data so the map has something to
/// show immediately. Replace both methods with real calls to your backend
/// (e.g. GET /api/clubs/{clubId}/locations and GET /api/locations/{id}) once
/// you have one - nothing else in the app needs to change, since everything
/// depends only on ILocationsService.
/// </summary>
public class LocationsService : ILocationsService
{
    private readonly IClubConfigService clubConfigService;

    // In a real implementation this would come from your database via an API call,
    // scoped to clubConfigService.Current (e.g. by a ClubId on the server).
    private static readonly List<ClubLocation> SampleLocations = new()
    {
        new ClubLocation
        {
            Id = 1,
            Name = "Boathouse Loop",
            Latitude = 51.4545,
            Longitude = -0.9781,
            ShortDescription = "Easy 5km loop from the boathouse.",
            FullDescription = "A flat, sheltered 5km loop starting and finishing at the boathouse. " +
                               "Suitable for all skill levels. Watch for the weir at the halfway point.",
            PhotoUrl = null,
            Files = new List<DownloadableFile>
            {
                new()
                {
                    FileName = "boathouse-loop.gpx",
                    Url = "https://example.com/files/boathouse-loop.gpx",
                    Description = "5km loop route"
                }
            }
        },
        new ClubLocation
        {
            Id = 2,
            Name = "Upper Reach",
            Latitude = 51.4602,
            Longitude = -0.9705,
            ShortDescription = "Longer 12km route, moderate.",
            FullDescription = "A 12km out-and-back route along the upper reach. Moderate current, " +
                               "recommended for members with at least six months' experience.",
            PhotoUrl = null,
            Files = new List<DownloadableFile>
            {
                new()
                {
                    FileName = "upper-reach.gpx",
                    Url = "https://example.com/files/upper-reach.gpx",
                    Description = "12km out-and-back route"
                }
            }
        }
    };

    public LocationsService(IClubConfigService clubConfigService)
    {
        this.clubConfigService = clubConfigService;
    }

    public Task<List<ClubLocation>> GetLocationsAsync()
    {
        return Task.FromResult(SampleLocations);
    }

    public Task<ClubLocation?> GetLocationAsync(int id)
    {
        return Task.FromResult(SampleLocations.FirstOrDefault(l => l.Id == id));
    }
}
