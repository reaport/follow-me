using System.Threading.Tasks;

namespace FollowMe.Services
{
    public interface IOrchestratorService
    {
        Task StartMovementAsync(string carId);
        Task EndMovementAsync(string carId);
    }
}