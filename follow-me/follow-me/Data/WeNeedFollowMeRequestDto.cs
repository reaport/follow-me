namespace FollowMe.Data
{
    public class WeNeedFollowMeRequestDto
    {
        public string AirplaneId { get; set; }
        public string NodeFrom { get; set; }
        public string NodeTo { get; set; }
        public bool IsTakeoff { get; set; } // Добавляем параметр IsTakeoff
    }
}