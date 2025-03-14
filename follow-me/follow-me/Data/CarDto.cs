namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий машину сопровождения.
    /// </summary>
    public class Car
    {
        /// <summary>
        /// Внутренний идентификатор машины.
        /// </summary>
        public string InternalId { get; set; }

        /// <summary>
        /// Внешний идентификатор машины.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Текущий статус машины.
        /// </summary>
        public CarStatusEnum Status { get; set; }

        /// <summary>
        /// Текущий узел, на котором находится машина.
        /// </summary>
        public string CurrentNode { get; set; }
    }
}