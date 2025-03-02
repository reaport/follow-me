namespace FollowMe.Data
{
    public class Car
    {
        public string InternalId { get; set; } // Внутренний ID (от 1 до n)
        public string ExternalId { get; set; } // Внешний ID (выдаваемый вышкой)
        public CarStatusEnum Status { get; set; }
    }
}