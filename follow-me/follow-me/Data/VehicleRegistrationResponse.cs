using System.Text.Json.Serialization;
namespace FollowMe.Data
{
    public class VehicleRegistrationResponse
    {
        [JsonPropertyName("garrageNodeId")]
        public string GarrageNodeId { get; set; }

        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; }

        [JsonPropertyName("serviceSpots")]
        public Dictionary<string, string> ServiceSpots { get; set; }
    }
}