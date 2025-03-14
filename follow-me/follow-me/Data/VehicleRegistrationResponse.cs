using System.Text.Json.Serialization;

namespace FollowMe.Data
{
    /// <summary>
    ///  ласс, представл€ющий ответ на запрос регистрации машины.
    /// </summary>
    public class VehicleRegistrationResponse
    {
        /// <summary>
        /// »дентификатор узла гаража, где зарегистрирована машина.
        /// </summary>
        [JsonPropertyName("garrageNodeId")]
        public string GarrageNodeId { get; set; }

        /// <summary>
        /// »дентификатор зарегистрированной машины.
        /// </summary>
        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; }

        /// <summary>
        /// —ловарь сервисных точек, св€занных с машиной.
        /// </summary>
        [JsonPropertyName("serviceSpots")]
        public Dictionary<string, string> ServiceSpots { get; set; }
    }
}