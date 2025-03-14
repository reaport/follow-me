namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий запрос на получение машины сопровождения.
    /// </summary>
    public class WeNeedFollowMeRequestDto
    {
        /// <summary>
        /// Идентификатор самолета, для которого требуется машина сопровождения.
        /// </summary>
        public string AirplaneId { get; set; }

        /// <summary>
        /// Начальный узел маршрута.
        /// </summary>
        public string NodeFrom { get; set; }

        /// <summary>
        /// Конечный узел маршрута.
        /// </summary>
        public string NodeTo { get; set; }

        /// <summary>
        /// Флаг, указывающий, тянет ли машина сопровождения самолет.
        /// </summary>
        public bool IsTakeoff { get; set; }
    }
}