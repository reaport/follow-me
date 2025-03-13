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


public class VehicleRegistrationResponse
    {
        [JsonPropertyName("garrageNodeId")]
        public string GarrageNodeId { get; set; }

        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; }

        [JsonPropertyName("serviceSpots")]
        public Dictionary<string, string> ServiceSpots { get; set; }
    }


    public class MoveResponse
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}