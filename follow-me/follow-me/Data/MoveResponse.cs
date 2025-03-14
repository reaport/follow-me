using System.Text.Json.Serialization;

namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий ответ на запрос перемещения.
    /// </summary>
    public class MoveResponse
    {
        /// <summary>
        /// Расстояние, которое необходимо преодолеть.
        /// </summary>
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}