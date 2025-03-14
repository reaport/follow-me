using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FollowMe.Services
{
    public interface IGroundControlService
    {
        Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType);
        Task<string[]> GetRoute(string from, string to);
        Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to, string withAirplane);
        Task NotifyArrival(string vehicleId, string vehicleType, string nodeId);
    }
}