using SeptaRail.ClientApp.Models;


namespace SeptaRail.ClientApp.Services;

public interface INextTrainFunction
{
    Task<List<NextTrain>> GetTasksAsync(NextTrainRequest request);
}
