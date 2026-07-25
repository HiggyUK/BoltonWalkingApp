using System.Text.Json;

namespace BoltonWalking.App.Services;

/// <summary>
/// Minimal read-only client for the Firestore REST API - no Firebase SDK
/// dependency, just HttpClient + JSON. Access is governed entirely by the
/// project's Firestore security rules (public read, no write), not by an
/// API key, so nothing sensitive needs to ship in the app.
/// </summary>
public class FirestoreClient
{
    private const string ProjectId = "bwoas-85868";
    private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

    private readonly HttpClient httpClient;

    public FirestoreClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>Returns each document in the collection as its raw "fields" JSON element, keyed by document id.</summary>
    public async Task<List<(string Id, JsonElement Fields)>> GetCollectionAsync(string collection)
    {
        var json = await httpClient.GetStringAsync($"{BaseUrl}/{collection}");
        using var parsed = JsonDocument.Parse(json);

        var results = new List<(string, JsonElement)>();
        if (!parsed.RootElement.TryGetProperty("documents", out var documents))
            return results;

        foreach (var document in documents.EnumerateArray())
        {
            var name = document.GetProperty("name").GetString() ?? "";
            var id = name[(name.LastIndexOf('/') + 1)..];
            var fields = document.TryGetProperty("fields", out var f) ? f.Clone() : default;
            results.Add((id, fields));
        }

        return results;
    }

    public static string GetString(JsonElement fields, string key, string fallback = "")
    {
        if (fields.ValueKind == JsonValueKind.Object &&
            fields.TryGetProperty(key, out var value) &&
            value.TryGetProperty("stringValue", out var s))
        {
            return s.GetString() ?? fallback;
        }
        return fallback;
    }

    public static string? GetNullableString(JsonElement fields, string key)
    {
        if (fields.ValueKind == JsonValueKind.Object &&
            fields.TryGetProperty(key, out var value) &&
            value.TryGetProperty("stringValue", out var s))
        {
            return s.GetString();
        }
        return null;
    }

    public static double GetDouble(JsonElement fields, string key)
    {
        if (fields.ValueKind != JsonValueKind.Object || !fields.TryGetProperty(key, out var value))
            return 0;

        if (value.TryGetProperty("doubleValue", out var d))
            return d.GetDouble();
        if (value.TryGetProperty("integerValue", out var i) && double.TryParse(i.GetString(), out var parsed))
            return parsed;
        return 0;
    }

    public static List<string> GetStringArray(JsonElement fields, string key)
    {
        var result = new List<string>();
        if (fields.ValueKind != JsonValueKind.Object || !fields.TryGetProperty(key, out var value))
            return result;
        if (!value.TryGetProperty("arrayValue", out var arr) || !arr.TryGetProperty("values", out var values))
            return result;

        foreach (var item in values.EnumerateArray())
            if (item.TryGetProperty("stringValue", out var s))
                result.Add(s.GetString() ?? "");

        return result;
    }

    /// <summary>Reads an array-of-maps field, handing each map's "fields" object to <paramref name="selector"/>.</summary>
    public static List<T> GetMapArray<T>(JsonElement fields, string key, Func<JsonElement, T> selector)
    {
        var result = new List<T>();
        if (fields.ValueKind != JsonValueKind.Object || !fields.TryGetProperty(key, out var value))
            return result;
        if (!value.TryGetProperty("arrayValue", out var arr) || !arr.TryGetProperty("values", out var values))
            return result;

        foreach (var item in values.EnumerateArray())
        {
            if (item.TryGetProperty("mapValue", out var map) && map.TryGetProperty("fields", out var mapFields))
                result.Add(selector(mapFields));
        }

        return result;
    }
}
