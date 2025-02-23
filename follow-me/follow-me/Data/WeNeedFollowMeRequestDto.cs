namespace FollowMe.Data
{
    public class WeNeedFollowMeRequestDto
    {
        public Guid AirplaneId { get; set; }
        public int FollowType { get; set; }
        public int GateNumber { get; set; }
        public int RunawayNumber { get; set; }
    }
}