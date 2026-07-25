using Clubs.Shared.Models;

namespace Clubs.Shared.Services;

public class ClubConfigService : IClubConfigService
{
    public ClubConfig Current { get; }

    // The ClubConfig instance is registered as a singleton in each App head's
    // MauiProgram.cs, so DI hands us the right one automatically - the shared
    // library never needs to know which club it's running for.
    public ClubConfigService(ClubConfig config)
    {
        Current = config;
    }
}
