using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

public interface IClubConfigService
{
    ClubConfig Current { get; }
}
