namespace FollowMe.Data
{
    public class Car
    {
        public string InternalId { get; set; }
        public string ExternalId { get; set; }
        public CarStatusEnum Status { get; set; }
        public string CurrentNode { get; set; }
    }
}