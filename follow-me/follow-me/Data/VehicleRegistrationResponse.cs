using System.Text.Json.Serialization;

namespace FollowMe.Data
{
    /// <summary>
    /// �����, �������������� ����� �� ������ ����������� ������.
    /// </summary>
    public class VehicleRegistrationResponse
    {
        /// <summary>
        /// ������������� ���� ������, ��� ���������������� ������.
        /// </summary>
        [JsonPropertyName("garrageNodeId")]
        public string GarrageNodeId { get; set; }

        /// <summary>
        /// ������������� ������������������ ������.
        /// </summary>
        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; }

        /// <summary>
        /// ������� ��������� �����, ��������� � �������.
        /// </summary>
        [JsonPropertyName("serviceSpots")]
        public Dictionary<string, string> ServiceSpots { get; set; }
    }
}