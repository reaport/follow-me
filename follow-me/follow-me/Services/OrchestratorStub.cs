using System.Threading.Tasks;
using FollowMe.Utils;

namespace FollowMe.Services
{
    public class OrchestratorStubService : IOrchestratorService
    {
        public Task StartMovementAsync(string carId, string aircraftId)
        {
            Logger.Log("OrchestratorStubService", "INFO", $"Заглушка: Запрос на начало движения для машины {carId}.");
            return Task.CompletedTask;
        }

        public Task EndMovementAsync(string carId, string aircraftId)
        {
            Logger.Log("OrchestratorStubService", "INFO", $"Заглушка: Запрос на окончание движения для машины {carId}.");
            return Task.CompletedTask;
        }
    }
}