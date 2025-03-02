namespace FollowMe.Data
{
    public class AuditDto
    {
        public DateTime Timestamp { get; set; }
        public string CarId { get; set; }
        public string Movement { get; set; }
    }
}