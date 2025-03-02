namespace FollowMe.Data
{
    public class WeNeedFollowMeRequestDto
    {
        public Guid AirplaneId { get; set; }
        public string NodeFrom { get; set; }
        public string NodeTo { get; set; }
    }
}