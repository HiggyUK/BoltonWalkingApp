using System.Text.Json;
using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

/// <summary>
/// Reads the 18 walking routes from the club's Firestore database (collection
/// "routes") via the Firestore REST API - see FirestoreClient. The club can
/// add/edit routes directly in the Firebase console and the app picks them up
/// on next load, no app update required.
/// </summary>
public class RoutesService : IRoutesService
{
    private readonly FirestoreClient firestoreClient;

    public RoutesService(FirestoreClient firestoreClient)
    {
        this.firestoreClient = firestoreClient;
    }

    public async Task<List<WalkingRoute>> GetRoutesAsync()
    {
        var documents = await firestoreClient.GetCollectionAsync("routes");
        return documents.Select(MapRoute).OrderBy(r => r.Id).ToList();
    }

    public async Task<WalkingRoute?> GetRouteAsync(int id)
    {
        var routes = await GetRoutesAsync();
        return routes.FirstOrDefault(r => r.Id == id);
    }

    private static WalkingRoute MapRoute((string Id, JsonElement Fields) document)
    {
        var (id, fields) = document;

        return new WalkingRoute
        {
            Id = int.TryParse(id, out var numericId) ? numericId : 0,
            Name = FirestoreClient.GetString(fields, "name"),
            Latitude = FirestoreClient.GetDouble(fields, "latitude"),
            Longitude = FirestoreClient.GetDouble(fields, "longitude"),
            Difficulty = Enum.TryParse<RouteDifficulty>(FirestoreClient.GetString(fields, "difficulty"), out var difficulty)
                ? difficulty
                : RouteDifficulty.Moderate,
            DifficultyBadge = FirestoreClient.GetString(fields, "difficultyBadge"),
            ShortDescription = FirestoreClient.GetString(fields, "shortDescription"),
            Stats = FirestoreClient.GetString(fields, "stats"),
            TerrainNotes = FirestoreClient.GetString(fields, "terrainNotes"),
            Venue = FirestoreClient.GetString(fields, "venue"),
            Address = FirestoreClient.GetString(fields, "address"),
            What3Words = FirestoreClient.GetString(fields, "what3Words"),
            GridReference = FirestoreClient.GetString(fields, "gridReference"),
            TransportNotes = FirestoreClient.GetString(fields, "transportNotes"),
            ParkingNotes = FirestoreClient.GetString(fields, "parkingNotes"),
            PhotoUrls = FirestoreClient.GetStringArray(fields, "photoUrls"),
            Files = FirestoreClient.GetMapArray(fields, "files", fileFields => new DownloadableFile
            {
                FileName = FirestoreClient.GetString(fileFields, "fileName"),
                Url = FirestoreClient.GetString(fileFields, "url"),
                Description = FirestoreClient.GetNullableString(fileFields, "description")
            })
        };
    }
}
