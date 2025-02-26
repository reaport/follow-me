using System.Collections.Generic;
using System.Threading.Tasks;

namespace FollowMe.Services
{
    public interface IGroundControlService
    {
        Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType);
        Task<string[]> GetRoute(string from, string to);
        Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to);
        Task NotifyArrival(string vehicleId, string vehicleType, string nodeId);
        Task SendNavigationSignal(string vehicleId, string signal);
    }

    public class VehicleRegistrationResponse
    {
        public string GarrageNodeId { get; set; }
        public string VehicleId { get; set; }
        public Dictionary<string, string> ServiceSpots { get; set; }
    }
}