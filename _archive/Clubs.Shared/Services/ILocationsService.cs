using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

public interface ILocationsService
{
    Task<List<ClubLocation>> GetLocationsAsync();
    Task<ClubLocation?> GetLocationAsync(int id);
}
