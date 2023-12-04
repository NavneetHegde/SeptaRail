using SeptaRail.ClientApp.Models;

namespace SeptaRail.ClientApp.Services;

public interface IRestService
{
    Task<List<NextTrain>> RefreshDataAsync(NextTrainRequest request);
}
