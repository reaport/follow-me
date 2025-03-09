using System.Threading.Tasks;

namespace FollowMe.Services
{
    public interface IOrchestratorService
    {
        Task StartMovementAsync(string carId, string aircraftId);
        Task EndMovementAsync(string carId, string aircraftId);
    }
}