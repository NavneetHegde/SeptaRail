using SeptaRail.ClientApp.Models;

namespace SeptaRail.ClientApp.Services;

public class NextTrainFunction : INextTrainFunction
{
    IRestService _restService;

    public NextTrainFunction(IRestService service)
    {
        _restService = service;
    }

    public Task<List<NextTrain>> GetTasksAsync(NextTrainRequest request)
    {
        return _restService.RefreshDataAsync(request);
    }
}
