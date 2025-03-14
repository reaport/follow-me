using System.Text.Json.Serialization;

namespace FollowMe.Data
{
    /// <summary>
    /// �����, �������������� ����� �� ������ �����������.
    /// </summary>
    public class MoveResponse
    {
        /// <summary>
        /// ����������, ������� ���������� ����������.
        /// </summary>
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}