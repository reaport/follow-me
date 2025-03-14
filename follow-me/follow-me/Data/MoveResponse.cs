namespace FollowMe.Data
{
    public class MoveResponse
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
}