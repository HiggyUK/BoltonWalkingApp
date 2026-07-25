using BoltonWalking.App.Models;

namespace BoltonWalking.App.Services;

public interface IRoutesService
{
    Task<List<WalkingRoute>> GetRoutesAsync();
    Task<WalkingRoute?> GetRouteAsync(int id);
}
